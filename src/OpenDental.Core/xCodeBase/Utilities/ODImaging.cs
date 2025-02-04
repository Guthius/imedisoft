using System.Drawing;
using System.Linq;

namespace CodeBase;

public class ODImaging
{
    public static void ImageApplyOrientation(Image image)
    {
        var propertyItem = image.PropertyItems.FirstOrDefault(x => x.Id == 0x0112);
        if (propertyItem is null || propertyItem.Value.Length <= 0)
        {
            return;
        }
        
        switch (propertyItem.Value[0])
        {
            case 6:
                image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                break;
            
            case 3:
                image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                break;
            
            case 8:
                image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                break;
        }
    }

    public static Bitmap ImageScaleMaxHeightAndWidth(Image image, long maxHeight, long maxWidth)
    {
        float scale = 1;
        
        if (image.PhysicalDimension.Height <= maxHeight && image.PhysicalDimension.Width <= maxWidth)
        {
            return new Bitmap(image, (int) (image.PhysicalDimension.Width * scale), (int) (image.PhysicalDimension.Height * scale));
        }
        
        if (image.PhysicalDimension.Width / maxWidth > image.PhysicalDimension.Height / maxHeight)
        {
            scale = maxWidth / image.PhysicalDimension.Width;
        }
        else
        {
            scale = maxHeight / image.PhysicalDimension.Height;
        }

        return new Bitmap(image, (int) (image.PhysicalDimension.Width * scale), (int) (image.PhysicalDimension.Height * scale));
    }
}