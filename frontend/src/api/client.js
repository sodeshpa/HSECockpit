import axios from "axios";

/**
 * Configured Axios instance for all API calls.
 * - Base URL from environment variable
 * - Request interceptor attaches Cognito JWT
 * - Response interceptor handles 401 and 5xx errors
 */
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

/**
 * Request interceptor: attach Cognito JWT token from session storage.
 * The token is retrieved from the Cognito user pool session stored in localStorage.
 */
apiClient.interceptors.request.use(
  async (config) => {
    try {
      const token = await getAuthToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    } catch {
      // If token retrieval fails, continue without auth header.
      // The 401 response interceptor will handle redirection.
    }
    return config;
  },
  (error) => Promise.reject(error)
);

/**
 * Response interceptor:
 * - 401: redirect to login (session expired or invalid)
 * - 5xx: log error for debugging
 */
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response) {
      const { status } = error.response;

      if (status === 401) {
        // Session expired or invalid — redirect to login
        window.location.href = "/login";
      }

      if (status >= 500) {
        console.error(
          `[API] Server error ${status}:`,
          error.response.data?.title || error.message
        );
      }
    }

    return Promise.reject(error);
  }
);

/**
 * Retrieve the current Cognito ID token from localStorage.
 * The amazon-cognito-identity-js library stores session tokens in localStorage
 * using a predictable key pattern.
 */
async function getAuthToken() {
  const userPoolId = import.meta.env.VITE_COGNITO_USER_POOL_ID;
  const clientId = import.meta.env.VITE_COGNITO_CLIENT_ID;

  if (!userPoolId || !clientId) {
    return null;
  }

  const lastAuthUserKey = `CognitoIdentityServiceProvider.${clientId}.LastAuthUser`;
  const lastAuthUser = localStorage.getItem(lastAuthUserKey);

  if (!lastAuthUser) {
    return null;
  }

  const idTokenKey = `CognitoIdentityServiceProvider.${clientId}.${lastAuthUser}.idToken`;
  return localStorage.getItem(idTokenKey);
}

export default apiClient;
