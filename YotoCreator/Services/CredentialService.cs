using System;
using System.Linq;
using Windows.Security.Credentials;

namespace YotoCreator.Services
{
    /// <summary>
    /// Service for securely storing and retrieving API credentials using Windows Credential Manager
    /// </summary>
    public class CredentialService
    {
        private readonly PasswordVault _vault;
        private const string CHATGPT_RESOURCE = "YotoCreator.ChatGPT";
        private const string YOTO_RESOURCE = "YotoCreator.Yoto";
        private const string DEFAULT_USERNAME = "ApiKey";

        public CredentialService()
        {
            _vault = new PasswordVault();
        }

        /// <summary>
        /// Saves the ChatGPT API key securely to Windows Credential Manager
        /// </summary>
        public void SaveChatGptKey(string apiKey)
        {
            SaveCredential(CHATGPT_RESOURCE, apiKey);
        }

        /// <summary>
        /// Retrieves the ChatGPT API key from Windows Credential Manager
        /// </summary>
        public string GetChatGptKey()
        {
            return GetCredential(CHATGPT_RESOURCE);
        }

        /// <summary>
        /// Removes the ChatGPT API key from Windows Credential Manager
        /// </summary>
        public void RemoveChatGptKey()
        {
            RemoveCredential(CHATGPT_RESOURCE);
        }

        /// <summary>
        /// Checks if a ChatGPT API key is stored
        /// </summary>
        public bool HasChatGptKey()
        {
            return HasCredential(CHATGPT_RESOURCE);
        }

        /// <summary>
        /// Saves the Yoto API key securely to Windows Credential Manager
        /// </summary>
        public void SaveYotoKey(string apiKey)
        {
            SaveCredential(YOTO_RESOURCE, apiKey);
        }

        /// <summary>
        /// Retrieves the Yoto API key from Windows Credential Manager
        /// </summary>
        public string GetYotoKey()
        {
            return GetCredential(YOTO_RESOURCE);
        }

        /// <summary>
        /// Removes the Yoto API key from Windows Credential Manager
        /// </summary>
        public void RemoveYotoKey()
        {
            RemoveCredential(YOTO_RESOURCE);
        }

        /// <summary>
        /// Checks if a Yoto API key is stored
        /// </summary>
        public bool HasYotoKey()
        {
            return HasCredential(YOTO_RESOURCE);
        }

        private void SaveCredential(string resource, string password)
        {
            try
            {
                // Remove existing credential if it exists
                RemoveCredential(resource);

                // Add new credential
                var credential = new PasswordCredential(resource, DEFAULT_USERNAME, password);
                _vault.Add(credential);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving credential for {resource}: {ex.Message}");
                throw;
            }
        }

        private string GetCredential(string resource)
        {
            try
            {
                var credentials = _vault.FindAllByResource(resource);
                if (credentials != null && credentials.Count > 0)
                {
                    var credential = credentials[0];
                    credential.RetrievePassword();
                    return credential.Password;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving credential for {resource}: {ex.Message}");
            }

            return null;
        }

        private void RemoveCredential(string resource)
        {
            try
            {
                var credentials = _vault.FindAllByResource(resource);
                if (credentials != null && credentials.Count > 0)
                {
                    foreach (var credential in credentials)
                    {
                        _vault.Remove(credential);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing credential for {resource}: {ex.Message}");
            }
        }

        private bool HasCredential(string resource)
        {
            try
            {
                var credentials = _vault.FindAllByResource(resource);
                return credentials != null && credentials.Count > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
