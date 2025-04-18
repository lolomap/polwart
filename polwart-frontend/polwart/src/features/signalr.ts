import { HubConnectionBuilder } from "@microsoft/signalr";
import * as api from '@/features/api';


const signalr = new HubConnectionBuilder()
    .withUrl('https://localhost:7238/notification')
    .build();

let isReady: boolean;
let isSubscribed: boolean;

export function OnLostConnection() {
    console.warn('DISCONNECTED FROM SIGNALR SERVER');
    isSubscribed = false;
    isReady = false;
}

export async function Init() {
    if (isReady) {
        console.warn('Multiply signalR init attemptions');
        return;
    }

    await signalr.start()
    .then(() => {
        signalr.on('RevisionsUpdate', (data) => {
            api.Update();
        });
        signalr.on('SubscribtionFailed', (data) => {
            console.error('Subscription failed:');
            console.error(data);
        });
        
        isReady = true;
    });
}

export function Subscribe(mapId: number) {
    if (!isReady) {
        console.error('SignalR is not ready');
        return;
    }

    signalr.invoke('Subscribe', mapId);
    isSubscribed = true;
}

export function Notify() {
    if (!isReady) {
        console.error('SignalR is not ready');
        return;
    }

    signalr.invoke('NotifyChanges');
}