const API_URL = import.meta.env.VITE_API_URL;

interface ApiFetchOptions extends RequestInit {
    accessToken?: string;
    options?: ApiFetchOptions
}

export async function apiFetch(
    path: string,
    options?: ApiFetchOptions
) {
    if (!API_URL) {
        throw new Error("VITE_API_URL is not configured.");
    }

    const headers = new Headers(options?.headers);

    if (!(options?.body instanceof FormData)) {
        headers.set("Content-Type", "application/json");
    }

    if (options?.accessToken) {
        headers.set("X-Player-Token", options.accessToken);
    }

    const response = await fetch(`${API_URL}${path}`, {
        ...options,
        headers,
    });

    if (!response.ok) {
        throw new Error(await response.text());
    }

    return response;
}