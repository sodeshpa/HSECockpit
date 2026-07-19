import { createContext, useState, useEffect, useCallback, useRef } from "react";
import {
  CognitoUserPool,
  CognitoUser,
  AuthenticationDetails,
} from "amazon-cognito-identity-js";
import { cognitoConfig } from "./cognitoConfig";

export const AuthContext = createContext(null);

const userPool = cognitoConfig.userPoolId && cognitoConfig.clientId
  ? new CognitoUserPool({
      UserPoolId: cognitoConfig.userPoolId,
      ClientId: cognitoConfig.clientId,
    })
  : null;

/**
 * AuthProvider wraps the application and provides authentication state
 * and operations via React Context.
 *
 * Exposes: user, isAuthenticated, isLoading, login(), logout(), getToken()
 */
export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(!userPool ? false : true);
  const refreshTimerRef = useRef(null);

  // If Cognito is not configured, render children with unauthenticated state
  if (!userPool) {
    const value = {
      user: null,
      isAuthenticated: false,
      isLoading: false,
      login: () => Promise.reject(new Error("Cognito is not configured")),
      logout: () => {},
      getToken: () => Promise.reject(new Error("Cognito is not configured")),
    };
    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
  }

  /**
   * Extract user info including custom:role from the current session.
   */
  const extractUserFromSession = useCallback((session) => {
    const idToken = session.getIdToken();
    const payload = idToken.decodePayload();
    return {
      username: payload["cognito:username"] || payload.sub,
      email: payload.email,
      role: payload["custom:role"] || null,
      sub: payload.sub,
    };
  }, []);

  /**
   * Schedule a token refresh before the access token expires.
   */
  const scheduleTokenRefresh = useCallback((session) => {
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current);
    }

    const accessToken = session.getAccessToken();
    const expiresAt = accessToken.getExpiration() * 1000;
    // Refresh 5 minutes before expiry
    const refreshIn = Math.max(expiresAt - Date.now() - 5 * 60 * 1000, 0);

    refreshTimerRef.current = setTimeout(() => {
      const currentUser = userPool.getCurrentUser();
      if (currentUser) {
        currentUser.getSession((err, newSession) => {
          if (!err && newSession && newSession.isValid()) {
            setUser(extractUserFromSession(newSession));
            scheduleTokenRefresh(newSession);
          }
        });
      }
    }, refreshIn);
  }, [extractUserFromSession]);

  /**
   * On mount, check if there is an existing valid session.
   */
  useEffect(() => {
    const currentUser = userPool.getCurrentUser();
    if (!currentUser) {
      setIsLoading(false);
      return;
    }

    currentUser.getSession((err, session) => {
      if (err || !session || !session.isValid()) {
        setIsLoading(false);
        return;
      }
      const userData = extractUserFromSession(session);
      setUser(userData);
      setIsAuthenticated(true);
      scheduleTokenRefresh(session);
      setIsLoading(false);
    });

    return () => {
      if (refreshTimerRef.current) {
        clearTimeout(refreshTimerRef.current);
      }
    };
  }, [extractUserFromSession, scheduleTokenRefresh]);

  /**
   * Authenticate a user with username and password.
   * Returns a promise that resolves with user data on success.
   */
  const login = useCallback((username, password) => {
    return new Promise((resolve, reject) => {
      const cognitoUser = new CognitoUser({
        Username: username,
        Pool: userPool,
      });

      const authDetails = new AuthenticationDetails({
        Username: username,
        Password: password,
      });

      cognitoUser.authenticateUser(authDetails, {
        onSuccess: (session) => {
          const userData = extractUserFromSession(session);
          setUser(userData);
          setIsAuthenticated(true);
          scheduleTokenRefresh(session);
          resolve(userData);
        },
        onFailure: (err) => {
          reject(err);
        },
      });
    });
  }, [extractUserFromSession, scheduleTokenRefresh]);

  /**
   * Sign out the current user and clear state.
   */
  const logout = useCallback(() => {
    const currentUser = userPool.getCurrentUser();
    if (currentUser) {
      currentUser.signOut();
    }
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current);
    }
    setUser(null);
    setIsAuthenticated(false);
  }, []);

  /**
   * Get the current valid ID token (JWT) for API authorization.
   * Returns a promise that resolves with the token string.
   */
  const getToken = useCallback(() => {
    return new Promise((resolve, reject) => {
      const currentUser = userPool.getCurrentUser();
      if (!currentUser) {
        reject(new Error("No authenticated user"));
        return;
      }

      currentUser.getSession((err, session) => {
        if (err) {
          reject(err);
          return;
        }
        if (!session || !session.isValid()) {
          reject(new Error("Session is invalid"));
          return;
        }
        resolve(session.getIdToken().getJwtToken());
      });
    });
  }, []);

  const value = {
    user,
    isAuthenticated,
    isLoading,
    login,
    logout,
    getToken,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
