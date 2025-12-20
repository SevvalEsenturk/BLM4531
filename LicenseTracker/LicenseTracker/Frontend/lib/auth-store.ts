import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface User {
    firstName: string;
    lastName: string;
    email: string;
    companyName?: string;
}

interface AuthState {
    token: string | null;
    user: User | null;
    setToken: (token: string) => void;
    setUser: (user: User) => void;
    logout: () => void;
    isAuthenticated: () => boolean;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set, get) => ({
            token: null,
            user: null,
            setToken: (token) => set({ token }),
            setUser: (user) => set({ user }),
            logout: () => {
                set({ token: null, user: null });
                localStorage.removeItem('auth-storage');
                sessionStorage.clear();
            },
            isAuthenticated: () => !!get().token,
        }),
        {
            name: 'auth-storage',
            partialize: (state) => ({ token: state.token, user: state.user }),
        }
    )
);
