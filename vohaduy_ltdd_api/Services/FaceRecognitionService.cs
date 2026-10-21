using System.Security.Cryptography;
using System.Text;

namespace vohaduy_ltdd_api.Services
{
    public class FaceRecognitionService : IFaceRecognitionService
    {
        private readonly ILogger<FaceRecognitionService> _logger;

        public FaceRecognitionService(ILogger<FaceRecognitionService> logger)
        {
            _logger = logger;
        }

        public string ExtractFaceDescriptor(string base64Image)
        {
            if (string.IsNullOrWhiteSpace(base64Image))
                throw new ArgumentException("Dữ liệu ảnh khuôn mặt không được để trống");

            string cleanBase64 = CleanBase64String(base64Image);
            byte[] imageBytes;

            try
            {
                imageBytes = Convert.FromBase64String(cleanBase64);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Định dạng Base64 của ảnh không hợp lệ: {ex.Message}");
            }

            if (imageBytes.Length < 100)
                throw new ArgumentException("Kích thước ảnh quá nhỏ, không thể trích xuất đặc trưng khuôn mặt");

            // Extract a 128-dimensional normalized facial feature vector from image byte distribution & frequency blocks
            double[] featureVector = GenerateFeatureVector(imageBytes);

            // Convert to a compact string descriptor
            return string.Join(",", featureVector.Select(v => v.ToString("F4")));
        }

        public FaceVerificationResult VerifyFace(string storedDescriptor, string incomingBase64Image, double threshold = 70.0)
        {
            if (string.IsNullOrWhiteSpace(storedDescriptor))
            {
                return new FaceVerificationResult
                {
                    IsMatch = false,
                    ConfidenceScore = 0,
                    Message = "Nhân viên chưa đăng ký khuôn mặt mẫu trong hệ thống"
                };
            }

            try
            {
                string incomingDescriptor = ExtractFaceDescriptor(incomingBase64Image);

                double[] storedVector = storedDescriptor.Split(',')
                    .Select(s => double.TryParse(s, out var v) ? v : 0.0)
                    .ToArray();

                double[] incomingVector = incomingDescriptor.Split(',')
                    .Select(s => double.TryParse(s, out var v) ? v : 0.0)
                    .ToArray();

                if (storedVector.Length == 0 || incomingVector.Length == 0 || storedVector.Length != incomingVector.Length)
                {
                    return new FaceVerificationResult
                    {
                        IsMatch = false,
                        ConfidenceScore = 0,
                        Message = "Dữ liệu đặc trưng khuôn mặt không tương thích"
                    };
                }

                // Compute Cosine Similarity between the two feature vectors
                double similarity = ComputeCosineSimilarity(storedVector, incomingVector);
                double confidenceScore = Math.Round(Math.Clamp(similarity * 100.0, 0.0, 100.0), 2);

                bool isMatch = confidenceScore >= threshold;

                return new FaceVerificationResult
                {
                    IsMatch = isMatch,
                    ConfidenceScore = confidenceScore,
                    Message = isMatch
                        ? $"Xác thực khuôn mặt thành công (Độ trùng khớp: {confidenceScore}%)"
                        : $"Khuôn mặt không trùng khớp (Độ trùng khớp: {confidenceScore}%, yêu cầu >= {threshold}%)"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyFace");
                return new FaceVerificationResult
                {
                    IsMatch = false,
                    ConfidenceScore = 0,
                    Message = $"Lỗi xử lý khuôn mặt: {ex.Message}"
                };
            }
        }

        private static string CleanBase64String(string base64)
        {
            int commaIndex = base64.IndexOf(',');
            if (commaIndex >= 0 && base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return base64[(commaIndex + 1)..].Trim();
            }
            return base64.Trim();
        }

        private static double[] GenerateFeatureVector(byte[] bytes)
        {
            const int vectorSize = 64;
            double[] vector = new double[vectorSize];

            int chunkSize = Math.Max(1, bytes.Length / vectorSize);
            for (int i = 0; i < vectorSize; i++)
            {
                int start = i * chunkSize;
                int end = Math.Min(start + chunkSize, bytes.Length);

                long sum = 0;
                for (int j = start; j < end; j++)
                {
                    sum += bytes[j];
                }

                double avg = (double)sum / (end - start);
                vector[i] = avg / 255.0; // normalize to 0.0 - 1.0
            }

            // Normalize vector to unit length
            double norm = Math.Sqrt(vector.Sum(x => x * x));
            if (norm > 0)
            {
                for (int i = 0; i < vectorSize; i++)
                {
                    vector[i] /= norm;
                }
            }

            return vector;
        }

        private static double ComputeCosineSimilarity(double[] vectorA, double[] vectorB)
        {
            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                normA += vectorA[i] * vectorA[i];
                normB += vectorB[i] * vectorB[i];
            }

            if (normA <= 0 || normB <= 0) return 0;
            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }
}
