import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5100/api', // HTTP port to avoid SSL issues
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor: Add Token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    if (token && token !== 'null' && token !== 'undefined') {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response Interceptor: Handle 401
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      // Optional: Redirect to login or clear token, but better handled in UI/Context
      if (typeof window !== 'undefined' && !window.location.pathname.includes('/login')) {
        // Uncomment to force redirect, or handle in component
        // window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default api;

export interface LicenseDto {
  id: number;
  name: string;
  vendor: string;
  hasLicense: boolean;
  startDate?: string;
  endDate?: string;
  cost?: number;
  users?: number;
  category: string;
  companyId?: number;
}

export interface VendorSyncResult {
  vendorName: string;
  success: boolean;
  licensesFound: number;
  licensesAdded: number;
  licensesUpdated: number;
  errorMessage?: string;
}

export const LicenseService = {
  getAll: async (): Promise<LicenseDto[]> => {
    const response = await api.get<LicenseDto[]>('/licenses');
    return response.data;
  },
  getById: async (id: number): Promise<LicenseDto> => {
    const response = await api.get<LicenseDto>(`/licenses/${id}`);
    return response.data;
  },
  create: async (license: LicenseDto): Promise<LicenseDto> => {
    const response = await api.post<LicenseDto>('/licenses', license);
    return response.data;
  },
  update: async (id: number, license: LicenseDto): Promise<void> => {
    await api.put(`/licenses/${id}`, license);
  },
  delete: async (id: number): Promise<void> => {
    await api.delete(`/licenses/${id}`);
  },
};

export const VendorSyncService = {
  syncAll: async (): Promise<VendorSyncResult[]> => {
    const response = await api.post<VendorSyncResult[]>('/vendor-sync/sync-all');
    return response.data;
  },
  syncVendor: async (vendorName: string): Promise<VendorSyncResult> => {
    const response = await api.post<VendorSyncResult>(`/vendor-sync/${vendorName}`);
    return response.data;
  },
};
