using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Imaging;
using OSGeo.GDAL;
using AppScene;

namespace GdalReader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string __ImagePath = string.Empty;
        private OSGeo.GDAL.Dataset __Geodataset;
        private int[] __DisplayBands;
        private Rectangle __DrawRect;
        private Bitmap __BitMap;
        private void btnBrower_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "";
            dlg.Filter = "Img(*.img)|*.img";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                OSGeo.GDAL.Gdal.AllRegister();
                __ImagePath = dlg.FileName;
                txtPath.Text = __ImagePath;
                OSGeo.GDAL.Dataset dataset = OSGeo.GDAL.Gdal.Open(__ImagePath, OSGeo.GDAL.Access.GA_ReadOnly);
                __Geodataset = dataset;
                if (__Geodataset != null)
                {
                    if (__Geodataset.RasterCount >= 3)
                        __DisplayBands = new int[3] { 1, 2, 3 };
                    else
                        __DisplayBands = new int[3] { 1, 1, 1 };
                }
                double[] dd = new double[4];
                dataset.GetGeoTransform(dd);
                string prj = dataset.GetProjection();

                string str = string.Format("波段数目:{0}\n行数：{1};列数：{2}\n坐标参考：{3},{4},{5},{6}\n", __Geodataset.RasterCount, __Geodataset.RasterXSize, __Geodataset.RasterYSize, dd[0], dd[1], dd[2], dd[3]);
                str += prj + "\n";
                for (int i = 1; i <= __Geodataset.RasterCount; ++i)
                {
                    OSGeo.GDAL.Band band = dataset.GetRasterBand(i);
                    str += "波段1：" + band.DataType.ToString();
                    //dataset.ReadRaster(
                }
                richTextBox1.Text = str;
                InitialIMG();
                SimpleRasterShow simRaster = new SimpleRasterShow("");
                simRaster.IsOn = true;
                simRaster.bitmap = __BitMap;
                sceneControl1.CurrentWorld.RenderableObjects.ChildObjects.Add(simRaster);
            }
        }

        public void InitialIMG()
        {
            if (__Geodataset != null)
            {
                Rectangle rect = new Rectangle(0, 0, __Geodataset.RasterXSize, __Geodataset.RasterYSize);
                float width = (float)this.Width;
                float height = (float)this.Height;
                RectangleF Extent = ExtRect(rect, width, height);
                double scale = Extent.Width / this.Width;
                //double scaley = Extent.Height / this.Height;
                double bufWidth = __Geodataset.RasterXSize / scale;
                double bufHeight = __Geodataset.RasterYSize / scale;
                Debug.WriteLine("Buffered width is:" + bufWidth);
                Debug.WriteLine("Buffered height is:" + bufHeight);
                double bufX = (this.Width - bufWidth) / 2.0;
                double bufY = (this.Height - bufHeight) / 2.0;
                __DrawRect = new Rectangle((int)bufX, (int)bufY, (int)bufWidth, (int)bufHeight);
                Rectangle ExtentRect = new Rectangle(0, 0, (int)bufWidth, (int)bufHeight);
                //__DispRectCenter = new PointF((float)(bufX + bufWidth / 2.0), (float)(bufY + bufHeight / 2.0));
                // __Zoom = (float)scale;
                //__Zoom=(float)(scalex>scaley?scalex:scaley);
                __BitMap = RSImg2BitMap(__Geodataset, ExtentRect, __DisplayBands);
                // Invalidate();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect">影像范围</param>
        /// <param name="width">窗体宽</param>
        /// <param name="height">窗体长</param>
        /// <returns></returns>
        public RectangleF ExtRect(Rectangle rect, float width, float height)
        {
            double midX = rect.X + rect.Width / 2.0;
            double midY = rect.Y + rect.Height / 2.0;
            double newh = 0.0;
            double neww = 0.0;
            //Adjust according to width, if 
            if (rect.Width * 1.0 / rect.Height > width / height)
            {
                newh = (height * 1.0 / width) * rect.Width;
                neww = rect.Width;
                //newh = (rect.Height*1.0 / rect.Width) * height;
                //neww = width;
            }
            else
            {
                //neww = (rect.Width*1.0 / rect.Height) * width;
                //newh = height;
                neww = (width * 1.0 / height) * rect.Width;
                newh = rect.Height;
            }
            RectangleF newRect = new RectangleF((float)(midX - neww / 2.0), (float)(midY - newh / 2.0), (float)neww, (float)newh);
            return newRect;
        }
        public Bitmap RSImg2BitMap(OSGeo.GDAL.Dataset dataset,
                                 Rectangle ExtentRect, int[] displayBands)
        {
            int x1width = ExtentRect.Width;
            int y1height = ExtentRect.Height;

            Bitmap image = new Bitmap(x1width, y1height,
                                     System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int iPixelSize = 3;

            if (dataset != null)
            {
                BitmapData bitmapdata = image.LockBits(new
                                        Rectangle(0, 0, x1width, y1height),
                                        ImageLockMode.ReadWrite, image.PixelFormat);
                int ch = 0;

                try
                {
                    unsafe
                    {
                        for (int i = 1; i <= displayBands.Length; ++i)
                        {
                            OSGeo.GDAL.Band band = dataset.GetRasterBand(displayBands[i - 1]);
                            int[] buffer = new int[x1width * y1height];
                            band.ReadRaster(0, 0, __Geodataset.RasterXSize,
                                __Geodataset.RasterYSize, buffer, x1width, y1height, 0, 0);
                            int p_indx = 0;

                            if ((int)band.GetRasterColorInterpretation() == 5)
                                ch = 0;
                            if ((int)band.GetRasterColorInterpretation() == 4)
                                ch = 1;
                            if ((int)band.GetRasterColorInterpretation() == 3)
                                ch = 2;
                            if ((int)band.GetRasterColorInterpretation() != 2)
                            {
                                double maxVal = 0.0;
                                double minVal = 0.0;
                                maxVal = GetMaxWithoutNoData(dataset,
                                         displayBands[i - 1], -9999.0);
                                minVal = GetMinWithoutNoData(dataset,
                                         displayBands[i - 1], -9999.0);
                                for (int y = 0; y < y1height; y++)
                                {
                                    byte* row = (byte*)bitmapdata.Scan0 +
                                                      (y * bitmapdata.Stride);
                                    for (int x = 0; x < x1width; x++, p_indx++)
                                    {
                                        byte tempVal = shift2Byte(buffer[p_indx], maxVal, minVal, -9999.0);
                                        row[x * iPixelSize + ch] = tempVal;
                                    }
                                }
                            }
                            else
                            {
                                double maxVal = 0.0;
                                double minVal = 0.0;
                                maxVal = GetMaxWithoutNoData(dataset,
                                         displayBands[i - 1], -9999.0);
                                minVal = GetMinWithoutNoData(dataset,
                                         displayBands[i - 1], -9999.0);
                                for (int y = 0; y < y1height; y++)
                                {
                                    byte* row = (byte*)bitmapdata.Scan0 +
                                                (y * bitmapdata.Stride);
                                    for (int x = 0; x < x1width; x++, p_indx++)
                                    {
                                        byte tempVal = shift2Byte<int>
                                                       (buffer[p_indx], maxVal, minVal, -9999.0);
                                        row[x * iPixelSize] = tempVal;
                                        row[x * iPixelSize + 1] = tempVal;
                                        row[x * iPixelSize + 2] = tempVal;
                                    }
                                }
                            }
                            ch++;
                        }
                    }
                }
                finally
                {
                    image.UnlockBits(bitmapdata);
                }
            }
            return image;
        }
        #region RASTERoperations
        /// <summary>
        /// Function of shift2Byte
        /// </summary>
        /// <remarks>this function will shift a value into a range of byte: 0~255 to be displayed in the graphics.</remarks>
        /// <typeparam name="T">the type of the value</typeparam>
        /// <param name="val">the value that will be converted to byte</param>
        /// <param name="Maximum">the maximum value range</param>
        /// <param name="Minimum">the minimum value range</param>
        /// <returns>a value within the byte range</returns>
        public byte shift2Byte<T>(T val, double Maximum, double Minimum)
        {
            double a = 255 / (Maximum - Minimum);
            double b = 255 - (255 / (Maximum - Minimum)) * Maximum;
            double tempVal = Convert.ToDouble(val);
            byte value = Convert.ToByte(a * tempVal + b);
            return value;
        }
        /// <summary>
        /// Function of shift2Byte
        /// </summary>
        /// <remarks>this function will shift a value into a range of byte: 0~255 to be displayed in the graphics.</remarks>
        /// <typeparam name="T">the type of the value</typeparam>
        /// <param name="val">the value that will be converted to byte</param>
        /// <param name="Maximum">the maximum value range</param>
        /// <param name="Minimum">the minimum value range</param>
        /// <param name="noData">the value for the non-sens pixel</param>
        /// <returns>a value within the byte range</returns>
        public byte shift2Byte<T>(T val, double Maximum, double Minimum, double noData)
        {
            double a = 0.0;
            double b = 0.0;
            double tempVal = Convert.ToDouble(val);
            a = 254 / (Maximum - Minimum);
            b = 255 - (254 / (Maximum - Minimum)) * Maximum;
            if (Math.Abs(tempVal) > Math.Abs(noData))
                return 0;
            try
            {
                return Convert.ToByte(a * tempVal + b);
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// Function of GetMaxWithoutNoData
        /// </summary>
        /// <remarks>Get the maximum data of certain band without the nodata values.</remarks>
        /// <param name="band">the band that will be statistically checked.</param>
        /// <returns>the maximum values.</returns>
        public double GetMaxWithoutNoData(OSGeo.GDAL.Dataset ds, int bandNumb, double __NoData)
        {
            double max = 0.0;
            double tempMax = 0.0;
            int index = 0;
            Band tempBand = ds.GetRasterBand(bandNumb);
            tempBand.GetMaximum(out tempMax, out index);
            if (Math.Abs(tempMax) < Math.Abs(__NoData))
                max = tempMax;
            else
            {
                OSGeo.GDAL.Band band;
                band = ds.GetRasterBand(bandNumb);
                //the number of columns
                int xSize = ds.RasterXSize;
                //the number of rows
                int ySize = ds.RasterYSize;
                double[] bandData = new double[xSize * ySize];
                //Read the data into the bandData matrix.
                OSGeo.GDAL.CPLErr err = band.ReadRaster(0, 0, xSize, ySize, bandData, xSize, ySize, 0, 0);
                for (long i = 0; i < xSize * ySize; i++)
                {
                    if (bandData[i] > max & (Math.Abs(bandData[i]) < Math.Abs(__NoData)))
                        max = bandData[i];
                }
            }
            return max;
        }
        /// <summary>
        /// Function of GetMinWithoutNoData
        /// </summary>
        /// <remarks>Get the maximum data of certain band without the nodata values.</remarks>
        /// <param name="band">the band that will be statistically checked</param>
        /// <returns>the maximum values.</returns>
        public double GetMinWithoutNoData(OSGeo.GDAL.Dataset ds, int bandNumb, double __NoData)
        {
            double min = Math.Abs(__NoData);
            double tempMin = 0.0;
            int index = 0;
            Band tempBand = ds.GetRasterBand(bandNumb);
            tempBand.GetMinimum(out tempMin, out index);
            if (Math.Abs(tempMin) < Math.Abs(__NoData))
                min = tempMin;
            else
            {
                OSGeo.GDAL.Band band;
                band = ds.GetRasterBand(bandNumb);
                //the number of columns
                int xSize = ds.RasterXSize;
                //the number of rows
                int ySize = ds.RasterYSize;
                double[] bandData = new double[xSize * ySize];
                //Read the data into the bandData matrix.
                OSGeo.GDAL.CPLErr err = band.ReadRaster(0, 0, xSize, ySize, bandData, xSize, ySize, 0, 0);
                for (long i = 0; i < xSize * ySize; i++)
                {
                    if (bandData[i] < min & (Math.Abs(bandData[i]) < Math.Abs(__NoData)))
                        min = bandData[i];
                }
            }
            return min;
        }
        /// <summary>
        /// Funcion of GetDatasetType
        /// </summary>
        /// <param name="band">the band where the data type will be defined.</param>
        /// <returns>0 is the byte, 1 is int, 2 is double, and 3 is unknown.</returns>
        public byte GetDatasetType(OSGeo.GDAL.Band band)
        {
            switch (band.DataType)
            {
                case OSGeo.GDAL.DataType.GDT_Byte:
                    return 0;
                case OSGeo.GDAL.DataType.GDT_CFloat32:
                case OSGeo.GDAL.DataType.GDT_CFloat64:
                case OSGeo.GDAL.DataType.GDT_Float32:
                case OSGeo.GDAL.DataType.GDT_Float64:
                    return 2;
                case OSGeo.GDAL.DataType.GDT_TypeCount:
                case OSGeo.GDAL.DataType.GDT_Unknown:
                    return 3;
                default:
                    return 1;
            }
        }
        #endregion
        private SceneControl sceneControl1;
        private void Form1_Load(object sender, EventArgs e)
        {
            this.sceneControl1 = new AppScene.SceneControl();
            // 
            // sceneControl1
            // 
            this.SuspendLayout();
            this.sceneControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sceneControl1.Location = new System.Drawing.Point(0, 0);
            this.sceneControl1.Name = "sceneControl1";
            this.sceneControl1.Size = new System.Drawing.Size(669, 457);
            this.sceneControl1.TabIndex = 0;
            this.panel1.Controls.Add(this.sceneControl1);
            sceneControl1.Focus();
            Application.Idle += new EventHandler(sceneControl1.OnApplicationIdle);
            this.ResumeLayout(false);
        }
    }
}
