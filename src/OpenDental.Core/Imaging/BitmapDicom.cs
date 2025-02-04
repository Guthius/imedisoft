using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using DataConnectionBase;
using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Codec;
using Dicom.Imaging.Render;

namespace OpenDentBusiness;

public class BitmapDicom
{
    public ushort[] ArrayDataRaw;
    public int Width;
    public int Height;
    public ushort BitsAllocated;
    public ushort BitsStored;
    public int WindowingMin;
    public int WindowingMax;

    public EnumDicomPhotometricInterp PhotometricInterp;
}

public class DicomHelper
{
    public static Bitmap ApplyWindowing(BitmapDicom bitmapDicom, int windowingMin, int windowingMax)
    {
        var bitmap = new Bitmap(bitmapDicom.Width, bitmapDicom.Height);
        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmapDicom.Width, bitmapDicom.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        //convert the windowing values to 12 bit versions, which does mean slightly rough increments for now.  This is not an issue because no data degradation.
        //4096 total values, from 0 to 4095.
        //255 x 16 = 4080, which is the max value we will support for windowing for now.
        unsafe
        {
            //rawData count should be same as pixels.  4 bytes per pixel
            var pBytes = (byte*) bitmapData.Scan0.ToPointer();
            for (var i = 0; i < bitmapDicom.ArrayDataRaw.Length; i++)
            {
                byte byteVal = 0;
                float floatDataRaw = bitmapDicom.ArrayDataRaw[i];
                if (bitmapDicom.BitsAllocated == 16)
                {
                    //12 or 16 bits stored
                    floatDataRaw = floatDataRaw / 16f;
                }

                if (floatDataRaw < windowingMin)
                {
                    byteVal = 0; //black
                }
                else if (floatDataRaw > windowingMax)
                {
                    byteVal = 255; //white
                }
                else
                {
                    if (bitmapDicom.BitsAllocated == 8)
                    {
                        byteVal = (byte) floatDataRaw;
                    }
                    else
                    {
                        //16 bit
                        byteVal = (byte) Math.Round(255f * (floatDataRaw - windowingMin) / (windowingMax - windowingMin));
                        //Examples using bytes/255: if window is .3 to .5 
                        //.301->.005, .4->.5, .499->.995,
                    }
                }

                //ARGB is stored as BGRA on little-endian machine. Alpha most significant, blue least.  Pointer to integer is stored the same.
                pBytes[i * 4] = byteVal; //B
                pBytes[i * 4 + 1] = byteVal; //G
                pBytes[i * 4 + 2] = byteVal; //R
                pBytes[i * 4 + 3] = 255; //A
            }
        }

        //Marshal.Copy(byteArray, 0, bitmapData.Scan0, byteArray.Length);// Bulk copy pixel data from a byte array
        //Marshal.WriteInt16(bitmapData.Scan0, offsetInBytes, shortValue);// Or, for one pixel at a time
        bitmap.UnlockBits(bitmapData);
        return bitmap;
    }

    public static BitmapDicom GetFromFile(string filename)
    {
        if (!File.Exists(filename))
        {
            return null;
        }

        var dicomFile = DicomFile.Open(filename);
        try
        {
            return GetFromDicomFile(dicomFile);
        }
        catch
        {
            return null;
        }
    }

    private static BitmapDicom GetFromDicomFile(DicomFile dicomFile)
    {
        /*
        When this method was created, we used example dicom images to figure out how to display them in Open Dental. Since then, we have received additional
        examples of different dicom images that were causing errors when viewing. This method has since been modified to support the new formats each time.
        The example images that were used can be found \\opendental.od\serverfiles\Engineering\dicom_image_examples

        Supporting a new format: Typically, when a new format is detected, an error from this method will occur. We should get an example image so we
        can change our code to handle the new format. Changes to our code will most like happen to this method(DicomHelper.GetFromDicomFile())
        and/or the DicomHelper.CalculateWindowingOnImport().

        After a new format has been added: After the code change for the newly supported format, make sure to add the example dicom image to the network
        \\opendental.od\serverfiles\Engineering\dicom_image_examples. Follow the existing pattern when creating the new directory.

        Debugging:
        Make sure to run the Open Dental solution in 'x86'. Running the solution in 'ANY CPU' will cause importing and viewing errors.
        */

        var bitmapDicom = new BitmapDicom();
        var dicomDataset = dicomFile.Dataset;
        var enumPhotometricInterp = SIn.Enum<EnumDicomPhotometricInterp>(dicomDataset.GetSingleValueOrDefault(new DicomTag(0x0028, 0x0004), ""), enumString: true, defaultValue: EnumDicomPhotometricInterp.None);
        if (enumPhotometricInterp == EnumDicomPhotometricInterp.None)
        {
            return null;
        }

        bitmapDicom.PhotometricInterp = enumPhotometricInterp;
        bitmapDicom.BitsAllocated = dicomDataset.GetSingleValueOrDefault<ushort>(new DicomTag(0x0028, 0x0100), 0);
        if (bitmapDicom.BitsAllocated != 16 && bitmapDicom.BitsAllocated != 8)
        {
            return null;
        }

        bitmapDicom.BitsStored = dicomDataset.GetSingleValueOrDefault<ushort>(new DicomTag(0x0028, 0x0101), 0);
        if (bitmapDicom.BitsStored != 12 && bitmapDicom.BitsStored != 8 && bitmapDicom.BitsStored != 16)
        {
            return null;
        }

        bitmapDicom.Height = dicomDataset.GetSingleValueOrDefault(new DicomTag(0x0028, 0x0010), 0); //rows
        if (bitmapDicom.Height == 0)
        {
            return null;
        }

        bitmapDicom.Width = dicomDataset.GetSingleValueOrDefault(new DicomTag(0x0028, 0x0011), 0); //columns
        if (bitmapDicom.Width == 0)
        {
            return null;
        }

        var dicomDatasetDecoded = dicomDataset.Clone(DicomTransferSyntax.ExplicitVRLittleEndian); //uses codec to decompress
        var dicomPixelDataDecoded = DicomPixelData.Create(dicomDatasetDecoded, false);
        var iPixelData = PixelDataFactory.Create(dicomPixelDataDecoded, 0);
        if (!(iPixelData is GrayscalePixelDataU16) && !(iPixelData is GrayscalePixelDataU8) && !(iPixelData is ColorPixelData24))
        {
            return null; //we cannot read other formats yet
        }

        bitmapDicom.ArrayDataRaw = new ushort[bitmapDicom.Height * bitmapDicom.Width];
        for (var r = 0; r < bitmapDicom.Height; r++)
        {
            for (var c = 0; c < bitmapDicom.Width; c++)
            {
                ushort pixelVal;
                if (iPixelData is ColorPixelData24)
                {
                    var colorCur = Color.FromArgb((int) iPixelData.GetPixel(c, r));
                    //Get the average RGB to convert to gray scale.
                    var intGrayScaleAverage = (colorCur.R + colorCur.G + colorCur.B) / 3;
                    pixelVal = (ushort) intGrayScaleAverage;
                }
                else
                {
                    pixelVal = (ushort) iPixelData.GetPixel(c, r);
                }

                bitmapDicom.ArrayDataRaw[r * bitmapDicom.Width + c] = pixelVal;
            }
        }

        return bitmapDicom;
    }

    public static void CalculateWindowingOnImport(BitmapDicom bitmapDicom)
    {
        if (bitmapDicom.BitsAllocated == 8)
        {
            bitmapDicom.WindowingMax = 255;
            return;
        }

        if (bitmapDicom.BitsStored == 16)
        {
            bitmapDicom.WindowingMax = 4095; //2^12
            return;
        }

        var histogram = new int[4096];
        for (var i = 0; i < bitmapDicom.ArrayDataRaw.Length; i++)
        {
            histogram[bitmapDicom.ArrayDataRaw[i]]++;
        }

        var idxBlackPixels = GetIndexForValue(histogram, histogram.Max()); //max value will hopefully be the black pixels.
        if (idxBlackPixels < 100)
        {
            //it's at the left, so it's almost certainly black
            histogram[idxBlackPixels] = 0; //throw out the large number of black pixels
        }

        var idxPeak = GetIndexForValue(histogram, histogram.Max());
        if (bitmapDicom.PhotometricInterp == EnumDicomPhotometricInterp.MONOCHROME1)
        {
            //MONOCHROME1 Grayscale ranges from bright to dark. Our logic below expects the grayscale to go from dark to bright.
            //Because of that, we need to hunt up(GetWindowingMax) to find the min, and hunt down(GetWindowingMin) to find the max.
            bitmapDicom.WindowingMin = GetWindowingMax(histogram, idxPeak);
            if (bitmapDicom.WindowingMin == 4095)
            {
                //Default value of max. Change to default value of min
                bitmapDicom.WindowingMin = 0;
            }

            bitmapDicom.WindowingMax = GetWindowingMin(histogram, idxPeak);
            if (bitmapDicom.WindowingMax == 0)
            {
                //Default value. Set to the actual max default value of 4095
                bitmapDicom.WindowingMax = 4095;
            }
        }
        else
        {
            bitmapDicom.WindowingMin = GetWindowingMin(histogram, idxPeak);
            bitmapDicom.WindowingMax = GetWindowingMax(histogram, idxPeak);
        }
    }

    private static int GetWindowingMin(int[] histogram, int idxPeak)
    {
        var threshold = 150;
        for (var i = idxPeak; i >= 0; i--)
        {
            //hunt down
            if (histogram[i] < threshold)
            {
                return i / 16; //converting from 12 bit to 8 bit
            }
        }

        return 0;
    }

    private static int GetWindowingMax(int[] histogram, int idxPeak)
    {
        var threshold = 150;
        for (var i = idxPeak; i < histogram.Length; i++)
        {
            //hunt up
            if (histogram[i] < threshold)
            {
                return i / 16; //converting from 12 bit to 8 bit
            }
        }

        return 4095;
    }

    private static int GetIndexForValue(int[] histogram, int value)
    {
        if (histogram == null)
        {
            return 0;
        }

        var idxOfValue = 0;
        for (var i = 0; i < histogram.Length; i++)
        {
            if (histogram[i] == value)
            {
                idxOfValue = i;
                break;
            }
        }

        return idxOfValue;
    }
}

public enum EnumDicomPhotometricInterp
{
    None = 0,
    MONOCHROME1,
    MONOCHROME2,
    RGB
}