using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace CodeBase;

public class ODImaging
{
    public static void ImageApplyOrientation(Image image)
    {
        PropertyItem propertyItem = image.PropertyItems.FirstOrDefault(x => x.Id == 0x0112); //Orientation tag
        if (propertyItem != null && propertyItem.Value.Length > 0)
        {
            //if(propertyItem.Value[0]==1)//no rotation. Do nothing
            if (propertyItem.Value[0] == 6)
            {
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            else if (propertyItem.Value[0] == 3)
            {
                image.RotateFlip(RotateFlipType.Rotate180FlipNone);
            }
            else if (propertyItem.Value[0] == 8)
            {
                image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }
        }
    }

    public static Bitmap ImageScaleMaxHeightAndWidth(Image image, long maxHeight, long maxWidth)
    {
        float imgScale = 1; //will be between 0 and 1
        if (image.PhysicalDimension.Height > maxHeight || image.PhysicalDimension.Width > maxWidth)
        {
            //image is too large
            //Image is larger than given constraints, resize to fit.
            if (image.PhysicalDimension.Width / maxWidth > image.PhysicalDimension.Height / maxHeight)
            {
                //resize image based on width
                imgScale = maxWidth / image.PhysicalDimension.Width;
            }
            else
            {
                //resize image based on height
                imgScale = maxHeight / image.PhysicalDimension.Height;
            }
        }

        return new Bitmap(image, (int) (image.PhysicalDimension.Width * imgScale), (int) (image.PhysicalDimension.Height * imgScale));
    }
}