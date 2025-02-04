using System.IO;
using System.Media;

namespace CodeBase;

public class SoundHelper
{
    public static void PlaySound(byte[] bytes)
    {
        using var memoryStream = new MemoryStream(bytes);
        using var soundPlayer = new SoundPlayer(memoryStream);
        
        soundPlayer.Play();
    }
}