/******************************************************************************
*   by Ha Xuan Tung
*   Email: tung.haxuancs@gmail.com
******************************************************************************
*   Please don't clear this comments
*   Copyright MTA 2017.
*   Learn more in site: https://sites.google.com/site/ictw666/
*   Youtube channel: https://goo.gl/Caj8Gj
*****************************************************************************/
#include "Recognizor.h"
#include "windows.h"

std::string wchar_t2string(const wchar_t *wchar)
{
	std::string str = "";
	int index = 0;
	while (wchar[index] != 0)
	{
		str += (char)wchar[index];
		++index;
	}
	return str;
}

wchar_t *string2wchar_t(const std::string &str)
{
	wchar_t wchar[260];
	int index = 0;
	while (index < str.size())
	{
		wchar[index] = (wchar_t)str[index];
		++index;
	}
	wchar[index] = 0;
	return wchar;
}
inline std::string getDirName(const std::string& filename)
{
	return filename.substr(0, filename.find_last_of('\\') + 1);
}

std::vector<std::string> getFilesInDirectory(const std::string& directory)
{
	WIN32_FIND_DATA FindFileData;
	wchar_t * FileName = string2wchar_t(directory);
	HANDLE hFind = FindFirstFile(FileName, &FindFileData);

	std::vector<std::string> listFileNames;
	listFileNames.push_back(getDirName(directory) + wchar_t2string(FindFileData.cFileName));

	while (FindNextFile(hFind, &FindFileData))
		listFileNames.push_back(getDirName(directory) + wchar_t2string(FindFileData.cFileName));
	return listFileNames;
}

/**
* Extract the class name from a file name
*/
inline std::string getClassName(const std::string& filename)
{
	int s = filename.find_last_of('\\');
	if (s == -1)
	{
		s = filename.find_last_of('/');
	}
	int e = filename.find_first_of('.');

	return filename.substr(s + 1, e - s - 1);
}

int getClassId(const std::set<std::string>& classes, const std::string& classname)
{
	int index = 0;
	for (auto it = classes.begin(); it != classes.end(); ++it)
	{
		if (*it == classname) break;
		++index;
	}
	return index;
}

/**
* Get a binary code associated to a class
*/
cv::Mat getClassCode(const std::set<std::string>& classes, const std::string& classname)
{
	cv::Mat code = cv::Mat::zeros(cv::Size((int)classes.size(), 1), CV_32F);
	int index = getClassId(classes, classname);
	code.at<float>(index) = 1;
	return code;
}

cv::Mat getBOWFeatures(cv::FlannBasedMatcher& flann, const cv::Mat& descriptors,
	int vocabularySize)
{
	cv::Mat outputArray = cv::Mat::zeros(cv::Size(vocabularySize, 1), CV_32F);
	std::vector<cv::DMatch> matches;
	flann.match(descriptors, matches);
	for (size_t j = 0; j < matches.size(); j++)
	{
		int visualWord = matches[j].trainIdx;
		outputArray.at<float>(visualWord)++;
	}
	return outputArray;
}

/**
* Get a trained neural network according to some inputs and outputs
*/
cv::Ptr<cv::ml::ANN_MLP> getTrainedNeuralNetwork(const cv::Mat& trainSamples,
	const cv::Mat& trainResponses)
{
	int networkInputSize = trainSamples.cols;
	int networkOutputSize = trainResponses.cols;
	cv::Ptr<cv::ml::ANN_MLP> mlp = cv::ml::ANN_MLP::create();
	std::vector<int> layerSizes = { networkInputSize, networkInputSize / 2,
		networkOutputSize };
	mlp->setLayerSizes(layerSizes);
	mlp->setActivationFunction(cv::ml::ANN_MLP::SIGMOID_SYM);
	mlp->train(trainSamples, cv::ml::ROW_SAMPLE, trainResponses);
	return mlp;
}

float getPredictedClass(const cv::Mat& predictions)
{
	float maxPrediction = predictions.at<float>(0);
	float maxPredictionIndex = 0;
	const float* ptrPredictions = predictions.ptr<float>(0);
	for (int i = 0; i < predictions.cols; i++)
	{
		float prediction = *ptrPredictions++;
		if (prediction > maxPrediction)
		{
			maxPrediction = prediction;
			maxPredictionIndex = i;
		}
	}
	return maxPrediction;
}

void printConfusionMatrix(const std::vector<std::vector<int> >& confusionMatrix,
	const std::set<std::string>& classes)
{
	for (auto it = classes.begin(); it != classes.end(); ++it)
	{
		std::cout << *it << " ";
	}
	std::cout << std::endl;
	for (size_t i = 0; i < confusionMatrix.size(); i++)
	{
		for (size_t j = 0; j < confusionMatrix[i].size(); j++)
		{
			std::cout << confusionMatrix[i][j] << " ";
		}
		std::cout << std::endl;
	}
}

/**
* Get the accuracy for a model (i.e., percentage of correctly predicted
* test samples)
*/
float getAccuracy(const std::vector<std::vector<int> >& confusionMatrix)
{
	int hits = 0;
	int total = 0;
	for (size_t i = 0; i < confusionMatrix.size(); i++)
	{
		for (size_t j = 0; j < confusionMatrix.at(i).size(); j++)
		{
			if (i == j) hits += confusionMatrix.at(i).at(j);
			total += confusionMatrix.at(i).at(j);
		}
	}
	return hits / (float)total;
}

/**
* Save our obtained models (neural network, bag of words vocabulary
* and class names) to use it later
*/
void saveModels(cv::Ptr<cv::ml::ANN_MLP> mlp, const cv::Mat& vocabulary,
	const std::set<std::string>& classes, const std::string& path)
{
	mlp->save(path + "\\mlp.yml");

	cv::FileStorage fs(path + "\\vocabulary.yaml", cv::FileStorage::WRITE);
	fs << "vocabulary" << vocabulary;
	fs.release();
	std::ofstream classesOutput(path + "\\classes.txt");
	for (auto it = classes.begin(); it != classes.end(); ++it)
	{
		classesOutput << getClassId(classes, *it) << "\t" << *it << std::endl;
	}
	classesOutput.close();
}

void loadModels(cv::Ptr<cv::ml::ANN_MLP>& mlp, cv::Mat& vocabulary,
	std::set<std::string>& classes, const std::string& path)
{
	/*FileStorage ffs("mlp.yml", FileStorage::READ);
	mlp = cv::Algorithm::read<cv::ml::ANN_MLP>(ffs.root());*/
	mlp = cv::Algorithm::load<cv::ml::ANN_MLP>(path + "\\mlp.yml");

	cv::FileStorage fs(path + "\\vocabulary.yaml", cv::FileStorage::READ);
	fs["vocabulary"] >> vocabulary;
	fs.release();

	std::ifstream classesIutput(path + "\\classes.txt", std::ifstream::in);
	if (classesIutput.is_open())
	{
		std::string line;
		while (getline(classesIutput, line))
		{
			std::string temp = line.substr(line.find_first_of('\t') + 1);
			classes.insert(temp);
		}
	}
	classesIutput.close();
}

KAZERecognizor::KAZERecognizor()
{
}
inline bool checkFileExist(const std::string& name)
{
	struct stat buffer;
	return (stat(name.c_str(), &buffer) == 0);
}

KAZERecognizor::KAZERecognizor(std::string path)
{
	pathImage = path;
	if (!checkFileExist(pathImage + "//mlp.yml"))
	{
		Train(pathImage, 32);
	}
	else
	{
		loadModels(mlp, vocabulary, classes, pathImage);
		flann.add(vocabulary);
		flann.train();
	}
}

cv::Mat KAZERecognizor::getDescriptors(const cv::Mat& img)
{
	cv::Ptr<cv::KAZE> kaze = cv::KAZE::create();
	std::vector<cv::KeyPoint> keypoints;
	cv::Mat descriptors;
	kaze->detectAndCompute(img, cv::noArray(), keypoints, descriptors);
	return descriptors;
}

void KAZERecognizor::readImages(vec_iter begin, vec_iter end, std::function<void(const std::string&, const cv::Mat&)> callback)
{
	for (auto it = begin; it != end; ++it)
	{
		std::string filename = *it;
		cv::Mat img = cv::imread(filename, 0);

		if (img.empty())
		{
			continue;
		}

		std::string classname = getClassName(filename);
		cv::Mat descriptors = getDescriptors(img);
		callback(classname, descriptors);
	}
}

void KAZERecognizor::Train(const std::string& imagesDirect, int netInputSize)
{
	std::string imagesDir = imagesDirect;
	networkInputSize = netInputSize;

	std::vector<std::string> files = getFilesInDirectory(imagesDir + "\\*.png");
	cv::Mat descriptorsSet;

	readImages(files.begin(), files.end(),
		[&](const std::string& classname, const cv::Mat& descriptors) {
		// Append to the set of classes
		classes.insert(classname);
		// Append to the list of descriptors
		descriptorsSet.push_back(descriptors);
		// Append metadata to each extracted feature
		ImageData* data = new ImageData;
		data->classname = classname;
		data->bowFeatures = cv::Mat::zeros(cv::Size(networkInputSize, 1), CV_32F);

		for (int j = 0; j < descriptors.rows; j++)
		{
			descriptorsMetadata.push_back(data);
		}
	});

	cv::Mat labels;
	// Use k-means to find k centroids (the words of our vocabulary)
	cv::kmeans(descriptorsSet, networkInputSize, labels, cv::TermCriteria(cv::TermCriteria::EPS +
		cv::TermCriteria::MAX_ITER, 10, 0.01), 1, cv::KMEANS_PP_CENTERS, vocabulary);
	// No need to keep it on memory anymore
	descriptorsSet.release();

	int* ptrLabels = (int*)(labels.data);
	int size = labels.rows * labels.cols;
	for (int i = 0; i < size; i++)
	{
		int label = *ptrLabels++;
		ImageData* data = descriptorsMetadata[i];
		data->bowFeatures.at<float>(label)++;
	}

	cv::Mat trainSamples;
	cv::Mat trainResponses;
	std::set<ImageData*> uniqueMetadata(descriptorsMetadata.begin(), descriptorsMetadata.end());
	for (auto it = uniqueMetadata.begin(); it != uniqueMetadata.end();)
	{
		ImageData* data = *it;
		cv::Mat normalizedHist;
		cv::normalize(data->bowFeatures, normalizedHist, 0, data->bowFeatures.rows, cv::NORM_MINMAX, -1, cv::Mat());
		trainSamples.push_back(normalizedHist);
		trainResponses.push_back(getClassCode(classes, data->classname));
		delete *it;
		it++;
	}
	descriptorsMetadata.clear();

	mlp = getTrainedNeuralNetwork(trainSamples, trainResponses);

	trainSamples.release();
	trainResponses.release();

	flann.add(vocabulary);
	flann.train();

	saveModels(mlp, vocabulary, classes, pathImage);
}

float KAZERecognizor::Predict(cv::Mat src)
{
	cv::Mat testSamples;
	cv::Mat descriptors = getDescriptors(src);

	cv::Mat bowFeatures = getBOWFeatures(flann, descriptors, networkInputSize);
	cv::normalize(bowFeatures, bowFeatures, 0, bowFeatures.rows, cv::NORM_MINMAX, -1, cv::Mat());
	testSamples.push_back(bowFeatures);

	cv::Mat testOutput;
	mlp->predict(testSamples, testOutput);

	return getPredictedClass(testOutput.row(0));
}