using System;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;

public class AWSRequestSigner : MonoBehaviour
{
    [Header("AWS Credentials")]
    public string accessKey = "YOUR_ACCESS_KEY";  // Replace with your Access Key
    public string secretKey = "YOUR_SECRET_KEY";  // Replace with your Secret Key
    public string region = "ap-south-1";
    public string service = "execute-api";
    public string apiUrl = "https://aco7wkvnqd.execute-api.ap-south-1.amazonaws.com/Prod/Project-CP";

    [Header("GameOver UI Elements")]
    public TMP_Text rankText;
    public TMP_Text scoreText;

    public void InsertHighScore(string playerId, int highScore)
    {
        string payload = "{\"action\":\"insert\",\"PlayerID\":\"" + playerId + "\",\"HighScore\":" + highScore + "}";

        StartCoroutine(SendSignedRequest(payload));
    }

    private IEnumerator SendSignedRequest(string payload)
    {
        string method = "POST";
        string host = new Uri(apiUrl).Host;
        string canonicalUri = "/Prod/Project-CP";
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        string date = DateTime.UtcNow.ToString("yyyyMMdd");

        // Canonical Request
        string canonicalHeaders = $"content-type:application/json\nhost:{host}\nx-amz-date:{timestamp}\n";
        string signedHeaders = "content-type;host;x-amz-date";
        string hashedPayload = ToHexString(Hash(Encoding.UTF8.GetBytes(payload)));
        string canonicalRequest = $"{method}\n{canonicalUri}\n\n{canonicalHeaders}\n{signedHeaders}\n{hashedPayload}";

        // String to Sign
        string algorithm = "AWS4-HMAC-SHA256";
        string credentialScope = $"{date}/{region}/{service}/aws4_request";
        string hashedCanonicalRequest = ToHexString(Hash(Encoding.UTF8.GetBytes(canonicalRequest)));
        string stringToSign = $"{algorithm}\n{timestamp}\n{credentialScope}\n{hashedCanonicalRequest}";

        // Signing Key and Signature
        byte[] signingKey = GetSignatureKey(secretKey, date, region, service);
        byte[] signature = HmacSHA256(signingKey, Encoding.UTF8.GetBytes(stringToSign));
        string signatureHex = ToHexString(signature);

        // Authorization Header
        string authorizationHeader = $"{algorithm} Credential={accessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signatureHex}";

        // Prepare UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(apiUrl, method);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-amz-date", timestamp);
        request.SetRequestHeader("Authorization", authorizationHeader);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Request successful: " + request.downloadHandler.text);
            ParseResponse(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Request failed: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    private void ParseResponse(string responseText)
    {
        // Assume the response contains a JSON object like:
        // {"Rank": 5, "PlayerID": "Player123", "HighScore": 1500}
        try
        {
            var response = JsonUtility.FromJson<HighScoreResponse>(responseText);

            // Update the UI
            rankText.text = $"Your Rank: {response.Rank}";
            scoreText.text = $"Your Score: {response.HighScore}";
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse response: " + e.Message);
        }
    }

    private static byte[] HmacSHA256(byte[] key, byte[] data)
    {
        using (var hmac = new HMACSHA256(key))
        {
            return hmac.ComputeHash(data);
        }
    }

    private static byte[] Hash(byte[] data)
    {
        using (var sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(data);
        }
    }

    private static string ToHexString(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    private static byte[] GetSignatureKey(string key, string date, string region, string service)
    {
        byte[] kDate = HmacSHA256(Encoding.UTF8.GetBytes("AWS4" + key), Encoding.UTF8.GetBytes(date));
        byte[] kRegion = HmacSHA256(kDate, Encoding.UTF8.GetBytes(region));
        byte[] kService = HmacSHA256(kRegion, Encoding.UTF8.GetBytes(service));
        return HmacSHA256(kService, Encoding.UTF8.GetBytes("aws4_request"));
    }

    [Serializable]
    private class HighScoreResponse
    {
        public int Rank;
        public int HighScore;
        public string PlayerID;
    }
}
