/**
 * Cognito configuration loaded from Vite environment variables.
 * Values are set in .env files (see .env.example for reference).
 */
export const cognitoConfig = {
  userPoolId: import.meta.env.VITE_COGNITO_USER_POOL_ID,
  clientId: import.meta.env.VITE_COGNITO_CLIENT_ID,
  domain: import.meta.env.VITE_COGNITO_DOMAIN,
  region: import.meta.env.VITE_AWS_REGION,
};
