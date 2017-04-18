/******************************************************************************
*   by Ha Xuan Tung
*   Email: tung.haxuancs@gmail.com
******************************************************************************
*   Please don't clear this comments
*   Copyright MTA 2017.
*   Learn more in site: https://sites.google.com/site/ictw666/
*   Youtube channel: https://goo.gl/Caj8Gj
*****************************************************************************/
#pragma once
#include "iostream"
#include "string"
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
class KAZERecognizor
{
	typedef std::vector<std::string>::const_iterator vec_iter;
public:
	KAZERecognizor();
	KAZERecognizor(std::string path);

	void Train(const std::string& imagesDirect, int netInputSize);
	float Predict(cv::Mat src);
private:
	cv::Mat getDescriptors(const cv::Mat& img);
	std::vector<ImageData*> descriptorsMetadata;
	std::set<std::string> classes;
	cv::FlannBasedMatcher flann;
	cv::Ptr<cv::ml::ANN_MLP> mlp;
	cv::Mat vocabulary;
	int networkInputSize = 32;

	std::string pathImage;

	void readImages(vec_iter begin, vec_iter end, std::function<void(const std::string&, const cv::Mat&)> callback);
};
