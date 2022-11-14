# PTI.Microservices.Library.Images

Facilitates editing images

**Examples:**

**Note: The examples below are passing null for the logger, if you want to use the logger make sure to pass the parameter with a value other than null**

## Fix Rotation
    ImagesService imagesService = new ImagesService(null);
    Image img = Image.FromFile(@"C:\Temp\iosScrewingImages.png", true);
    imagesService.RotateImageByExifOrientationData(img);
    img.Save(@"C:\Temp\FixediOSScrew.png");

## Crop
    ImagesService imagesService = new ImagesService(null);
    var imageStream = File.OpenRead(@"C:\Temp\TestImageToCrop.png");
    var croppedImageBytes = imagesService.Crop(imageStream, 600, 400, 200, 100);