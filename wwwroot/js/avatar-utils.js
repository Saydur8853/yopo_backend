/**
 * Avatar Utility Functions
 * Shared functions for handling user avatars with profile photos and initials fallback
 */

/**
 * Get user initials from first and last name
 * @param {string} firstName - User's first name
 * @param {string} lastName - User's last name
 * @returns {string} User initials (e.g., "JD" for John Doe)
 */
function getInitials(firstName, lastName) {
    const first = firstName ? firstName.charAt(0).toUpperCase() : '';
    const last = lastName ? lastName.charAt(0).toUpperCase() : '';
    return first + last || 'U';
}

/**
 * Generate avatar HTML with profile photo or initials fallback
 * @param {Object} user - User object containing profile information
 * @param {string} user.firstName - User's first name
 * @param {string} user.lastName - User's last name
 * @param {string} user.profilePhotoBase64 - Base64 encoded profile photo
 * @param {string} size - Size class (e.g., 'small', 'medium', 'large')
 * @param {string} additionalClasses - Additional CSS classes to apply
 * @returns {string} HTML string for avatar
 */
function generateAvatarHtml(user, size = 'medium', additionalClasses = '') {
    if (!user) return '<div class="user-avatar">U</div>';
    
    const initials = getInitials(user.firstName, user.lastName);
    const fullName = `${user.firstName || ''} ${user.lastName || ''}`.trim();
    const sizeClass = `avatar-${size}`;
    
    if (user.profilePhotoBase64) {
        return `
            <div class="user-avatar ${sizeClass} ${additionalClasses}">
                <img src="${user.profilePhotoBase64}" 
                     alt="${fullName} profile photo" 
                     onerror="this.parentNode.innerHTML='${initials}';">
            </div>`;
    } else {
        return `<div class="user-avatar ${sizeClass} ${additionalClasses}">${initials}</div>`;
    }
}

/**
 * Update an existing avatar element with profile photo or initials
 * @param {HTMLElement} avatarElement - The avatar DOM element to update
 * @param {Object} user - User object containing profile information
 */
function updateAvatarElement(avatarElement, user) {
    if (!avatarElement || !user) return;
    
    const initials = getInitials(user.firstName, user.lastName);
    const fullName = `${user.firstName || ''} ${user.lastName || ''}`.trim();
    
    if (user.profilePhotoBase64) {
        avatarElement.innerHTML = `<img src="${user.profilePhotoBase64}" 
                                       alt="${fullName} profile photo" 
                                       onerror="this.parentNode.innerHTML='${initials}';">`;
    } else {
        avatarElement.innerHTML = initials;
    }
}

/**
 * Fallback function when avatar image fails to load
 * @param {HTMLImageElement} img - The image element that failed to load
 * @param {string} initials - Fallback initials to display
 */
function showAvatarFallback(img, initials) {
    if (img && img.parentNode) {
        img.parentNode.innerHTML = initials || 'U';
    }
}

/**
 * Create avatar CSS styles for different sizes
 * @returns {string} CSS styles for avatar components
 */
function getAvatarStyles() {
    return `
        .user-avatar {
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 600;
            overflow: hidden;
            position: relative;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }

        .user-avatar img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            border-radius: 50%;
        }

        .avatar-small {
            width: 30px;
            height: 30px;
            font-size: 0.8rem;
        }

        .avatar-medium {
            width: 40px;
            height: 40px;
            font-size: 1rem;
        }

        .avatar-large {
            width: 50px;
            height: 50px;
            font-size: 1.2rem;
        }

        .avatar-xl {
            width: 80px;
            height: 80px;
            font-size: 2rem;
        }

        .avatar-xxl {
            width: 120px;
            height: 120px;
            font-size: 2.5rem;
        }
    `;
}

// Export functions for use in other modules (if using module system)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        getInitials,
        generateAvatarHtml,
        updateAvatarElement,
        showAvatarFallback,
        getAvatarStyles
    };
}