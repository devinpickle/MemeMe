public class ImageCleanupService
{
    public void DeleteGameImages(string joinCode)
    {
        var uploadPath = Path.Combine(
            "Assets",
            "Uploads",
            joinCode
        );

        if (Directory.Exists(uploadPath))
        {
            Directory.Delete(uploadPath, recursive: true);
        }
    }
}