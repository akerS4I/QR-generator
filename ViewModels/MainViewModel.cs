using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QR_generator.Services;

namespace QR_generator.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IQrCodeService _qrService;
    private readonly IImageSaveService _saveService;
    private byte[]? _qrBytes;

    public MainViewModel(IQrCodeService qrService, IImageSaveService saveService)
    {
        _qrService = qrService;
        _saveService = saveService;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    private string link = string.Empty;

    [ObservableProperty]
    private ImageSource? qrImage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatus))]
    private string? statusMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private void Generate()
    {
        StatusMessage = null;

        var text = Link.Trim();
        if (!text.Contains("://"))
            text = "https://" + text;

        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            ErrorMessage = "Enter a valid link.";
            SetQr(null);
            return;
        }

        ErrorMessage = null;
        SetQr(_qrService.Generate(uri.AbsoluteUri));
    }

    private bool CanGenerate() => !string.IsNullOrWhiteSpace(Link);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (_qrBytes is null)
            return;

        try
        {
            var fileName = $"qr_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var saved = await _saveService.SaveAsync(_qrBytes, fileName);
            StatusMessage = saved
                ? "Saved to Gallery (Pictures/QR Generator)."
                : "Saving works on Android 10 or newer.";
        }
        catch (Exception)
        {
            StatusMessage = "Could not save the QR code.";
        }
    }

    private bool CanSave() => _qrBytes is not null;

    [RelayCommand]
    private void Clear()
    {
        Link = string.Empty;
        ErrorMessage = null;
        StatusMessage = null;
        SetQr(null);
    }

    private void SetQr(byte[]? bytes)
    {
        _qrBytes = bytes;
        QrImage = bytes is null
            ? null
            : ImageSource.FromStream(() => new MemoryStream(bytes));
        SaveCommand.NotifyCanExecuteChanged();
    }
}