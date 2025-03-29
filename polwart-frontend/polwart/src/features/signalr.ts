import { HubConnectionBuilder } from "@microsoft/signalr";


const signalr = new HubConnectionBuilder()
    .withUrl('https://localhost:7238/notification')
    .build();

let isReady: boolean;
let isSubscribed: boolean;

export function OnLostConnection() {
    isSubscribed = false;
}

export function Init() {
    if (isReady) {
        console.warn('Multiply signalR init attemptions');
        return;
    }

    signalr.start()
    .then(() => {
        signalr.on('RevisionsUpdate', (data) => {console.log('Notification')});
        signalr.on('SubscribtionFailed', (data) => {console.log(data)});
        
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