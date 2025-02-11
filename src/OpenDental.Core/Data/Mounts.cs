using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Mounts
{
    public static List<Mount> GetPatientData(long patNum)
    {
        return MountCrud.SelectMany("SELECT * FROM mount WHERE PatNum=" + patNum + " ORDER BY DateCreated");
    }

    public static long Insert(Mount mount)
    {
        return MountCrud.Insert(mount);
    }

    public static void Update(Mount mount)
    {
        MountCrud.Update(mount);
    }

    public static void Delete(Mount mount)
    {
        Db.NonQ("DELETE FROM mount WHERE MountNum = " + mount.MountNum);
        Db.NonQ("DELETE FROM mountitem WHERE MountNum = " + mount.MountNum);
    }

    public static Mount GetByNum(long mountNum)
    {
        return MountCrud.SelectOne(mountNum) ?? new Mount();
    }

    public static Mount CreateMountFromDef(MountDef mountDef, long patNum, long docCategory)
    {
        var mount = new Mount
        {
            PatNum = patNum,
            DocCategory = docCategory,
            DateCreated = DateTime.Now,
            Description = mountDef.Description,
            Note = "",
            Width = mountDef.Width,
            Height = mountDef.Height,
            ColorBack = mountDef.ColorBack,
            ColorFore = mountDef.ColorFore,
            ColorTextBack = mountDef.ColorTextBack,
            FlipOnAcquire = mountDef.FlipOnAcquire,
            AdjModeAfterSeries = mountDef.AdjModeAfterSeries
        };

        mount.MountNum = Insert(mount);

        var mountItemDefs = MountItemDefs.GetForMountDef(mountDef.MountDefNum);

        foreach (var mountItemDef in mountItemDefs)
        {
            MountItems.Insert(new MountItem
            {
                MountNum = mount.MountNum,
                Xpos = mountItemDef.Xpos,
                Ypos = mountItemDef.Ypos,
                ItemOrder = mountItemDef.ItemOrder,
                Width = mountItemDef.Width,
                Height = mountItemDef.Height,
                RotateOnAcquire = mountItemDef.RotateOnAcquire,
                ToothNumbers = mountItemDef.ToothNumbers,
                TextShowing = mountItemDef.TextShowing,
                FontSize = mountItemDef.FontSize
            });
        }

        if (mountDef.ScaleValue != "")
        {
            ImageDraws.Insert(new ImageDraw
            {
                MountNum = mount.MountNum,
                DrawType = ImageDrawType.ScaleValue,
                DrawingSegment = mountDef.ScaleValue
            });
        }

        return mount;
    }

    public static Bitmap GetThumbnail(long mountNum, string patFolder)
    {
        var path = Path.Combine(patFolder, "Thumbnails");

        if (!Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch
            {
                return Documents.NoAvailablePhoto();
            }
        }

        var fileName = "Mount" + mountNum + ".jpg";
        var fileNameFull = Path.Combine(patFolder, "Thumbnails", fileName);

        Bitmap bitmap = null;
        if (!File.Exists(fileNameFull))
        {
            return Documents.NoAvailablePhoto();
        }

        try
        {
            bitmap = (Bitmap) Image.FromFile(fileNameFull);
        }
        catch
        {
            try
            {
                File.Delete(fileNameFull);
            }
            catch
            {
                return Documents.NoAvailablePhoto();
            }
        }

        if (bitmap is null)
        {
            return Documents.NoAvailablePhoto();
        }

        var bitmap2 = new Bitmap(bitmap);
        
        bitmap.Dispose();
        
        return bitmap2;
    }

    public static string ReplaceMount(string stringOriginal, Mount mount, bool isHtmlEmail = false)
    {
        if (mount == null)
        {
            return stringOriginal;
        }

        var stringBuilder = new StringBuilder(stringOriginal);

        ReplaceTags.ReplaceOneTag(stringBuilder, "[MountDate]", mount.DateCreated.ToShortDateString(), isHtmlEmail);
        ReplaceTags.ReplaceOneTag(stringBuilder, "[MountDescript]", mount.Description, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(stringBuilder, "[MountProv]", Providers.GetFormalName(mount.ProvNum), isHtmlEmail);

        return stringBuilder.ToString();
    }
}