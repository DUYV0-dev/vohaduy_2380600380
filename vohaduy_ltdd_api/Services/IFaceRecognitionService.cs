namespace vohaduy_ltdd_api.Services
{
    public class FaceVerificationResult
    {
        public bool IsMatch { get; set; }
        public double ConfidenceScore { get; set; } // 0 - 100%
        public string? Message { get; set; }
    }

    public interface IFaceRecognitionService
    {
        string ExtractFaceDescriptor(string base64Image);
        FaceVerificationResult VerifyFace(string storedDescriptor, string incomingBase64Image, double threshold = 70.0);
    }
}
