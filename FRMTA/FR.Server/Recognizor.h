#pragma once
#include "iostream"
#include "fstream"
#include <algorithm>
#include <functional>
#include <map>
#include <set>
#include "opencv.hpp"


struct ImageData
{
	std::string classname;
	cv::Mat bowFeatures;
};
struct PredictResults
{
	std::vector<std::string> predicted;
	std::vector<std::string> expected;
	std::vector<std::string> path;
	float accuracy;
};
class KAZERecognizor
{
	typedef std::vector<std::string>::const_iterator vec_iter;
public:
	KAZERecognizor();
	cv::Mat getDescriptors(const cv::Mat& img);

	void Train(const std::string& imagesDirect, int netInputSize);
	std::string Predict(const cv::Mat& src);
private:

	std::vector<ImageData*> descriptorsMetadata;
	std::set<std::string> classes;
	cv::FlannBasedMatcher flann;
	cv::Ptr<cv::ml::ANN_MLP> mlp;
	cv::Mat vocabulary;
	int networkInputSize = 32;

	void readImages(vec_iter begin, vec_iter end, std::function<void(const std::string&, const cv::Mat&)> callback);
};

