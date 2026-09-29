import * as signalR from "@microsoft/signalr";

const API_URL = import.meta.env.VITE_API_URL;

export const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_URL}/gamehub`)
    .withAutomaticReconnect()
    .build();