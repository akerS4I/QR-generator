#if ANDROID
using Android.Content;
using Android.Provider;
#endif

namespace QR_generator.Services;

public interface IImageSaveService
{
    /// <summary>Saves a PNG image to the device gallery. Returns true on success.</summary>
    Task<bool> SaveAsync(byte[] png, string fileName);
}

public class ImageSaveService : IImageSaveService
{
    public async Task<bool> SaveAsync(byte[] png, string fileName)
    {
#if ANDROID
        // Saving through MediaStore needs no permission on Android 10 (API 29) and newer.
        if (!OperatingSystem.IsAndroidVersionAtLeast(29))
            return false;

        var resolver = Android.App.Application.Context.ContentResolver;
        if (resolver is null)
            return false;

        var values = new ContentValues();
        values.Put(MediaStore.IMediaColumns.DisplayName, fileName);
        values.Put(MediaStore.IMediaColumns.MimeType, "image/png");
        values.Put(MediaStore.IMediaColumns.RelativePath, "Pictures/QR Generator");

        var uri = resolver.Insert(MediaStore.Images.Media.ExternalContentUri!, values);
        if (uri is null)
            return false;

        using var stream = resolver.OpenOutputStream(uri);
        if (stream is null)
            return false;

        await stream.WriteAsync(png);
        await stream.FlushAsync();
        return true;
#else
        await Task.CompletedTask;
        return false;
#endif
    }
}
