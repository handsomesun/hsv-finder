using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Drawing.Imaging;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Data.Services.Client;
using System.Drawing;

using Emgu.CV;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Microsoft.Kinect;


namespace KinectCam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor myKinect;

        int HsvCircleCount = 0;
        int RgbCircleCount = 0;

        //values for HSV
        int H_min = 0;
        int H_max = 179;
        int S_min = 0;
        int S_max = 255;
        int V_min = 0;
        int V_max = 255;

        //values for HSV
        int R_min = 0;
        int R_max = 255;
        int B_min = 0;
        int B_max = 255;
        int G_min = 0;
        int G_max = 255;
         
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            myKinect = KinectSensor.KinectSensors[0];

            myKinect.ColorStream.Enable(ColorImageFormat.YuvResolution640x480Fps15);
            
            myKinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(myKinect_ColorFrameReady);
            myKinect.ColorStream.CameraSettings.Contrast = 2.0;
            myKinect.ColorStream.CameraSettings.Gamma = 2.8;
            //myKinect.ColorStream.CameraSettings.Saturation = 2.0;
            myKinect.ColorStream.CameraSettings.Brightness = 0.3;

            myKinect.Start();
        }
        Image<Bgr, Byte> imgBgr;
        Image<Hsv, Byte> imgHsv;
        Image<Gray, Byte> processedHsv;
        Image<Gray, Byte> processedBgr;
        Bitmap bmap;
        Gray cannyThreshold = new Gray(160);
        Gray cannyThresholdLinking = new Gray(120);
        Gray circleAccumulatorThreshold = new Gray(60);

        void myKinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null) return;


                bmap = OpenCV2WPFConverter.ColorImageFrameToBitmap(colorFrame);

                imgBgr = new Image<Bgr, Byte>(bmap);
                imgHsv = new Image<Hsv, Byte>(bmap);

                if (imgBgr == null || imgHsv == null) return;
                processedBgr = imgBgr.InRange(new Bgr(B_min, G_min, R_min), new Bgr(B_max, G_max, R_max));

                processedHsv = imgHsv.InRange(new Hsv(H_min, S_min, V_min), new Hsv(H_max, S_max, V_max));
                //0,130,0 ~ 120, 256, 120 for green color.
                processedBgr = processedBgr.SmoothGaussian(7);
                processedHsv = processedHsv.SmoothGaussian(7);


                CircleF[] circlesBgr = processedBgr.HoughCircles(cannyThreshold, circleAccumulatorThreshold
                    , 2, processedBgr.Height / 8 , 8, 40)[0];

                CircleF[] circlesHsv = processedBgr.HoughCircles(cannyThreshold, circleAccumulatorThreshold
                    , 2, processedHsv.Height / 8, 8, 40)[0];

                HsvCircleCount = 0;
                RgbCircleCount = 0;

                // Draw Circles for RBG video stream 
                foreach (CircleF circle in circlesBgr)
                {

                    RgbCircleCount += 1;
                    imgBgr.Draw(circle, new Bgr(System.Drawing.Color.Bisque), 3);
                    
                }

                // Draw Circles for HSV video stream 
                foreach (CircleF circle in circlesHsv)
                {

                    HsvCircleCount += 1;
                    imgBgr.Draw(circle, new Bgr(System.Drawing.Color.Bisque), 3);

                }


                kinectVideo.Source = OpenCV2WPFConverter.ToBitmapSource(imgBgr);
                HsvVideo.Source = OpenCV2WPFConverter.ToBitmapSource(processedHsv);
                RgbVideo.Source = OpenCV2WPFConverter.ToBitmapSource(processedBgr);
                //control the distance of different circles!
                this.HsvCircleUI.Content = HsvCircleCount.ToString();
                this.RgbCircleUI.Content = RgbCircleCount.ToString();

            }
        }

        private void Slider_ValueChanged_H_min(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.H_min_value != null)
            {
                H_min = (int)e.NewValue;
                this.H_min_value.Content = H_min;
            }
        }

        private void Slider_ValueChanged_H_max(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.H_max_value != null)
            {
                H_max = (int)e.NewValue;
                this.H_max_value.Content = H_max;
            }
        }

        private void Slider_ValueChanged_S_min(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.S_min_value != null)
            {
                S_min = (int)e.NewValue;
                this.S_min_value.Content = S_min;
            }
        }

        private void Slider_ValueChanged_S_max(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.S_max_value != null)
            {
                S_max = (int)e.NewValue;
                this.S_max_value.Content = S_max;
            }
        }

        private void Slider_ValueChanged_V_min(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.V_min_value != null)
            {
                V_min = (int)e.NewValue;
                this.V_min_value.Content = V_min;
            }
        }

        private void Slider_ValueChanged_V_max(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.V_max_value != null)
            {
                V_max = (int)e.NewValue;
                this.V_max_value.Content = V_max;
            }
        }


        private void Slider_ValueChanged_R_min(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.R_min_value != null)
            {
                R_min = (int)e.NewValue;
                this.R_min_value.Content = R_min;
            }
        }

        private void Slider_ValueChanged_R_max(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.R_max_value != null)
            {
                R_max = (int)e.NewValue;
                this.R_max_value.Content = R_max;
            }
        }

        private void Slider_ValueChanged_B_min(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.B_min_value != null)
            {
                B_min = (int)e.NewValue;
                this.B_min_value.Content = B_min;
            }
        }

        private void Slider_ValueChanged_B_max(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.B_max_value != null)
            {
                B_max = (int)e.NewValue;
                this.B_max_value.Content = B_max;
            }
        }

        private void Slider_ValueChanged_G_min(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.G_min_value != null)
            {
                G_min = (int)e.NewValue;
                this.G_min_value.Content = G_min;
            }
        }

        private void Slider_ValueChanged_G_max(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.G_max_value != null)
            {
                G_max = (int)e.NewValue;
                this.G_max_value.Content = G_max;
            }
        }
    }
}
