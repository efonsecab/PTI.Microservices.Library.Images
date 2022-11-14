using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTI.Microservices.Library.Services
{
    /// <summary>
    /// Handles Images
    /// </summary>
    public class ImagesService
    {
        private ILogger<ImagesService> Logger { get; }
        /// <summary>
        /// Creates a new instance of <see cref="ImagesService"/>
        /// </summary>
        /// <param name="logger"></param>
        public ImagesService(ILogger<ImagesService> logger)
        {
            this.Logger = logger;
        }

        /// <summary>
        /// Crops an image.
        /// Source: https://stackoverflow.com/questions/51955644/how-can-i-crop-an-image-in-net-core
        /// </summary>
        /// <param name="imageStream"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="graphicsUnit"></param>
        /// <returns></returns>
        public byte[] Crop(Stream imageStream, float Width, float Height, float X, float Y,
            GraphicsUnit graphicsUnit = GraphicsUnit.Pixel)
        {
            try
            {
                using (Image OriginalImage = Image.FromStream(imageStream))
                {
                    using (Bitmap bmp = new Bitmap((int)Math.Ceiling(Width), (int)Math.Ceiling(Height)))
                    {
                        bmp.SetResolution(OriginalImage.HorizontalResolution, OriginalImage.VerticalResolution);

                        using (Graphics Graphic = Graphics.FromImage(bmp))
                        {
                            Graphic.SmoothingMode = SmoothingMode.AntiAlias;

                            Graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                            Graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            Graphic.DrawImage(OriginalImage,
                                destRect: new RectangleF(0,0,bmp.Width, bmp.Height),
                                srcRect: new RectangleF(X,Y, Width, Height), srcUnit:graphicsUnit);

                            MemoryStream ms = new MemoryStream();

                            bmp.Save(ms, OriginalImage.RawFormat);

                            return ms.GetBuffer();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Rotate the given image file according to Exif Orientation data
        /// </summary>
        /// <param name="sourceFilePath">path of source file</param>
        /// <param name="targetFilePath">path of target file</param>
        /// <param name="targetFormat">target format</param>
        /// <param name="updateExifData">set it to TRUE to update image Exif data after rotation (default is TRUE)</param>
        /// <returns>The RotateFlipType value corresponding to the applied rotation. If no rotation occurred, RotateFlipType.RotateNoneFlipNone will be returned.</returns>
        public RotateFlipType RotateImageByExifOrientationData(string sourceFilePath, string targetFilePath, 
            ImageFormat targetFormat, bool updateExifData = true)
        {
            // Rotate the image according to EXIF data
            var bmp = new Bitmap(sourceFilePath);
            RotateFlipType fType = RotateImageByExifOrientationData(bmp, updateExifData);
            if (fType != RotateFlipType.RotateNoneFlipNone)
            {
                bmp.Save(targetFilePath, targetFormat);
            }
            return fType;
        }

        /// <summary>
        /// Rotate the given bitmap according to Exif Orientation data
        /// </summary>
        /// <param name="img">source image</param>
        /// <param name="updateExifData">set it to TRUE to update image Exif data after rotation (default is TRUE)</param>
        /// <returns>The RotateFlipType value corresponding to the applied rotation. If no rotation occurred, RotateFlipType.RotateNoneFlipNone will be returned.</returns>
        public RotateFlipType RotateImageByExifOrientationData(Image img, bool updateExifData = true)
        {
            int orientationId = 0x0112;
            var fType = RotateFlipType.RotateNoneFlipNone;
            if (img.PropertyIdList.Contains(orientationId))
            {
                var pItem = img.GetPropertyItem(orientationId);
                fType = GetRotateFlipTypeByExifOrientationData(pItem.Value[0]);
                if (fType != RotateFlipType.RotateNoneFlipNone)
                {
                    img.RotateFlip(fType);
                    // Remove Exif orientation tag (if requested)
                    if (updateExifData) img.RemovePropertyItem(orientationId);
                }
            }
            return fType;
        }

        /// <summary>
        /// Return the proper System.Drawing.RotateFlipType according to given orientation EXIF metadata
        /// </summary>
        /// <param name="orientation">Exif "Orientation"</param>
        /// <returns>the corresponding System.Drawing.RotateFlipType enum value</returns>
        public RotateFlipType GetRotateFlipTypeByExifOrientationData(int orientation)
        {
            switch (orientation)
            {
                case 1:
                default:
                    return RotateFlipType.RotateNoneFlipNone;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
            }
        }

        private System.Drawing.Image ResizeAndDraw(System.Drawing.Image objTempImage)
        {
            // call image helper to fix the orientation issue 
            var temp = this.RotateImageByExifOrientationData(objTempImage, true);
            Size objSize = new Size(150, 200);
            Bitmap objBmp;
            objBmp = new Bitmap(objSize.Width, objSize.Height);

            Graphics g = Graphics.FromImage(objBmp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //Rectangle rect = new Rectangle(x, y, thumbSize.Width, thumbSize.Height);
            Rectangle rect = new Rectangle(0, 0, 150, 200);
            //g.DrawImage(objTempImage, rect, 0, 0, objTempImage.Width, objTempImage.Height, GraphicsUnit.Pixel);
            g.DrawImage(objTempImage, rect);
            return objBmp;
        }
    }
}
