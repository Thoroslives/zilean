namespace Zilean.Shared.Features.Configuration;

public class KubernetesSelector
{
    public string UrlTemplate { get; set; } = "";
    public string LabelSelector { get; set; } = "";
    public GenericEndpointType EndpointType { get; set; } = GenericEndpointType.Zurg;
}
