using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class Mounts
{
    
    public static List<Mount> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM mount WHERE PatNum=" + SOut.Long(patNum)
                                                          + " ORDER BY DateCreated";
        return MountCrud.SelectMany(command);
    }

    public static long Insert(Mount mount)
    {
        return MountCrud.Insert(mount);
    }

    public static void Update(Mount mount)
    {
        MountCrud.Update(mount);
    }


    ///<summary>Should already have checked that no images are attached.</summary>
    public static void Delete(Mount mount)
    {
        var command = "DELETE FROM mount WHERE MountNum='" + SOut.Long(mount.MountNum) + "'";
        Db.NonQ(command);
        command = "DELETE FROM mountitem WHERE MountNum='" + SOut.Long(mount.MountNum) + "'";
        Db.NonQ(command);
    }

    ///<summary>Returns a single mount object corresponding to the given mount number key.</summary>
    public static Mount GetByNum(long mountNum)
    {
        var mount = MountCrud.SelectOne(mountNum);
        if (mount == null) return new Mount();
        return mount;
    }

    ///<summary>MountItems are included.  Everything has been inserted into the database.</summary>
    public static Mount CreateMountFromDef(MountDef mountDef, long patNum, long docCategory)
    {
        var mount = new Mount();
        mount.PatNum = patNum;
        mount.DocCategory = docCategory; //this was already decided one level up
        mount.DateCreated = DateTime.Now;
        mount.Description = mountDef.Description;
        mount.Note = "";
        mount.Width = mountDef.Width;
        mount.Height = mountDef.Height;
        mount.ColorBack = mountDef.ColorBack;
        mount.ColorFore = mountDef.ColorFore;
        mount.ColorTextBack = mountDef.ColorTextBack;
        //mount.ScaleValue=mountDef.ScaleValue;//see below
        mount.FlipOnAcquire = mountDef.FlipOnAcquire;
        mount.AdjModeAfterSeries = mountDef.AdjModeAfterSeries;
        mount.MountNum = Insert(mount);
        //mount.ListMountItems=new List<MountItem>();
        var listMountItemDefs = MountItemDefs.GetForMountDef(mountDef.MountDefNum);
        for (var i = 0; i < listMountItemDefs.Count; i++)
        {
            var mountItem = new MountItem();
            mountItem.MountNum = mount.MountNum;
            mountItem.Xpos = listMountItemDefs[i].Xpos;
            mountItem.Ypos = listMountItemDefs[i].Ypos;
            mountItem.ItemOrder = listMountItemDefs[i].ItemOrder;
            mountItem.Width = listMountItemDefs[i].Width;
            mountItem.Height = listMountItemDefs[i].Height;
            mountItem.RotateOnAcquire = listMountItemDefs[i].RotateOnAcquire;
            mountItem.ToothNumbers = listMountItemDefs[i].ToothNumbers;
            mountItem.TextShowing = listMountItemDefs[i].TextShowing;
            mountItem.FontSize = listMountItemDefs[i].FontSize;
            mountItem.MountItemNum = MountItems.Insert(mountItem);
            //mount.ListMountItems.Add(mountItem);
        }

        if (mountDef.ScaleValue != "")
        {
            var imageDraw = new ImageDraw();
            imageDraw.MountNum = mount.MountNum;
            imageDraw.DrawType = ImageDrawType.ScaleValue;
            imageDraw.DrawingSegment = mountDef.ScaleValue;
            ImageDraws.Insert(imageDraw);
        }

        return mount;
    }

    /// <summary>
    ///     Gets the thumbnail image for the given mount. Like documents, the thumbnail for every mount is in a subfolder
    ///     named 'Thumbnails' within each patient's images folder.  For now, they will be 100x100, although we might change
    ///     that. Thumbnail file names are "Mount"+MountNum+".jpg". Example: ..\SmithJohn425\Thumbnails\Mount382.jpg.
    /// </summary>
    public static Bitmap GetThumbnail(long mountNum, string patFolder)
    {
        //Create Thumbnails folder if it does not already exist for this patient folder.
        var pathThumbnails = Path.Combine(patFolder, "Thumbnails");
        if (true && !Directory.Exists(pathThumbnails))
            try
            {
                Directory.CreateDirectory(pathThumbnails);
            }
            catch
            {
                return Documents.NoAvailablePhoto();
            }

        var fileName = "Mount" + mountNum + ".jpg";
        var fileNameFull = Path.Combine(patFolder, "Thumbnails", fileName);
        //Use the existing thumbnail if it already exists
        //(no way currently to check how old it is)
        Bitmap bitmap = null;
        if (true && File.Exists(fileNameFull))
        {
            try
            {
                bitmap = (Bitmap) Image.FromFile(fileNameFull);
            }
            catch
            {
                try
                {
                    File.Delete(fileNameFull); //File may be invalid, corrupted, or unavailable. This was a bug in previous versions.
                }
                catch
                {
                    //we tried our best, and it just wasn't good enough
                    return Documents.NoAvailablePhoto();
                }
            }

            //but that bitmap has a file lock that we need to release.
            if (bitmap != null)
            {
                var bitmap2 = new Bitmap(bitmap);
                bitmap?.Dispose();
                return bitmap2;
            }
        }

        //Unlike documents, this method never creates a missing thumbnail because that would cause delays.
        //Creation happens in ControlImageDisplay.CreateMountThumbnail().
        return Documents.NoAvailablePhoto();
    }

    ///<summary>Replaces </summary>
    public static string ReplaceMount(string stringOriginal, Mount mount, bool isHtmlEmail = false)
    {
        if (mount == null) return stringOriginal;
        var stringBuilder = new StringBuilder(stringOriginal);
        ReplaceTags.ReplaceOneTag(stringBuilder, "[MountDate]", mount.DateCreated.ToShortDateString(), isHtmlEmail);
        ReplaceTags.ReplaceOneTag(stringBuilder, "[MountDescript]", mount.Description, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(stringBuilder, "[MountProv]", Providers.GetFormalName(mount.ProvNum), isHtmlEmail);
        return stringBuilder.ToString();
    }
}