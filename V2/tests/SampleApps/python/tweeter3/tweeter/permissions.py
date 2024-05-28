from rest_framework import permissions


class IsSelfOrAdmin(permissions.BasePermission):
    """
    Object-level permission to only only the current User,
    or an admin User, to view and edit a User.
    """

    def has_object_permission(self, request, view, obj):
        # A User can edit and view their own data
        is_self = obj == request.user
        is_admin = request.user.is_superuser
        return is_self or is_admin


class IsAuthorOrReadOnly(permissions.BasePermission):
    """
    Permission that allows only the author to edit
    tweets attributed to them
    """
    def has_object_permission(self, request, view, obj):
        if request.method in permissions.SAFE_METHODS:
            # Allow read only permissions to any user
            # to view the tweet
            return True
        else:
            # Check that the request user owns the object
            # being edited
            return obj.user == request.user
