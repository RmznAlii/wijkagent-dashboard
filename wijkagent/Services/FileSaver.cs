using Microsoft.JSInterop;

namespace WijkAgent.Services;

public class FileSaver
{
    private readonly IJSRuntime _js;

    public FileSaver(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SaveAsync(string filename, byte[] data)
    {
        var base64 = Convert.ToBase64String(data);
        await _js.InvokeVoidAsync("saveFileFromBytes", filename, base64);
    }
}