// Simple Frontend Configuration
// Detect environment
const hostname = window.location.hostname;
const isProduction = hostname !== 'localhost' && hostname !== '127.0.0.1' && !hostname.startsWith('192.168.');

// Environment-specific configurations
const config = {
    API_BASE: '/api',
    DEBUG: !isProduction,
    ENVIRONMENT: isProduction ? 'production' : 'development'
};

// Global configuration object
window.AppConfig = {
    get: function(key) {
        return config[key];
    },
    
    getApiBase: function() {
        return config.API_BASE;
    },
    
    isProduction: function() {
        return config.ENVIRONMENT === 'production';
    },
    
    isDevelopment: function() {
        return config.ENVIRONMENT === 'development';
    },
    
    isDebugMode: function() {
        return config.DEBUG;
    },
    
    // Enhanced error handling for production
    handleError: function(error, context) {
        context = context || '';
        if (config.DEBUG) {
            console.error(context ? '[' + context + ']' : '', error);
        } else {
            console.warn('An error occurred' + (context ? ' in ' + context : ''));
        }
    },
    
    // Network error handling with retry logic
    fetchWithRetry: async function(url, options, maxRetries) {
        options = options || {};
        maxRetries = maxRetries || 2;
        let lastError;
        
        for (let attempt = 0; attempt <= maxRetries; attempt++) {
            try {
                const response = await fetch(url, options);
                
                // If successful, return response
                if (response.ok || response.status < 500) {
                    return response;
                }
                
                // For server errors, throw to retry
                throw new Error('Server error: ' + response.status);
                
            } catch (error) {
                lastError = error;
                
                // Don't retry on client errors (4xx) or authentication errors
                if (error.message && (
                    error.message.includes('401') || 
                    error.message.includes('403') ||
                    error.message.includes('400')
                )) {
                    throw error;
                }
                
                // If this is the last attempt, throw the error
                if (attempt === maxRetries) {
                    break;
                }
                
                // Wait before retrying (exponential backoff)
                await new Promise(function(resolve) {
                    setTimeout(resolve, Math.pow(2, attempt) * 1000);
                });
            }
        }
        
        throw lastError;
    }
};

// Legacy support - keep existing API_BASE constant working
window.API_BASE = config.API_BASE;
