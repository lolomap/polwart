import { useSessionStore } from '@/entities/store';
import * as signalr from '@/features/signalr';

let session: any;

const backendUrl = import.meta.env.VITE_BACKEND_URL;
const mediaUrl = import.meta.env.VITE_MEDIA_URL;

let MapId: number = -1;
let LastUpdateTimestamp = 0;

export type UpdateEvent = {layer: number, index: number, value: any, type: string};

export const Events = new EventTarget();

export async function MediaUpload(fileName: string, file: File) {
    fetch(mediaUrl + `/${fileName}`, {
        method: 'PUT',
        body: file
    })
    .then((response) => {});
}

export function GetMapImageAddress(): string {
    return mediaUrl + `/mapBG_${MapId}`;
};

export async function Create(isPublic: boolean, initialTimestampISO: string, bgFormat: string) {
    await fetch(backendUrl + '/map/create', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(
            {
                isPublic: isPublic,
                initialTimestampISO: initialTimestampISO,
                backgroundFormat: bgFormat
            }
        )
    })
    .then(response => response.json())
    .then(data => {
        MapId = data.mapId;
    });

    return MapId;
}

export async function Connect(mapId: number) {
    await signalr.Init();

    session = useSessionStore();
    MapId = mapId;
    let status: number;
    let map = null;

    await fetch(backendUrl + '/map/connect', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(
            {
                mapId: MapId
            }
        )
    })
    .then(response => {
        status = response.status;
        return response.json();
    })
    .then((data) => {
        // TODO: check success connection status before trying to read and subscribe
        session.mapData = data.root;
        map = data.mapInfo;

        data.revisions.forEach((revision: string) => {
            session.patch(revision);
        });

        //console.log(session.mapData);

        signalr.Subscribe(MapId);
    });

    return map;
};

export function Patch(patches: string) {
    if (MapId < 0) {
        console.error('Connect before patching!');
        return;
    }

    fetch(backendUrl + '/map/patch', {
        method: 'PATCH',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(
            {
                'mapId': MapId,
                'timestamp': Date.now(),
                'patch': patches
            }
        )
    })
    .then(response => response.status)
    .then(status => {
        if (status == 200)
            signalr.Notify();
    });
};

export function Update() {
    if (MapId < 0) {
        console.error('Connect before patching!');
        return;
    }

    fetch(backendUrl + '/map/update', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(
            {
                mapId: MapId,
                sinceTimestamp: LastUpdateTimestamp
            }
        )
    })
    .then(result => result.json())
    .then(data => {
        if (!Array.isArray(data)) {
            console.error('Unexpected response');
            console.log(data);
            return;
        }

        //console.log(data);

        data.forEach((revision: string) => {
            // Apply revision to local Map data
            session.patch(revision);

            // Apply revision to graph UI
            const patches = JSON.parse(revision);
            patches.forEach((patch: any) => {
                const path: string[] = patch.path.split('/');
            
                if (path[1] == 'layers') {
                    let event: UpdateEvent = {
                        layer: Number.parseInt(path[2]),
                        index: Number.parseInt(path[4]),
                        value: patch.value,
                        type: ''
                    };

                    if (path[path.length - 1] == 'x') {
                        event.type = 'x';
                    }
                    else if (path[path.length - 1] == 'y') {
                        event.type = 'y';
                    }
                    else {
                        event.type = patch.op;
                    }

                    Events.dispatchEvent(new CustomEvent('symbolUpdated', {detail: event}));
                }
            });
        });

        console.log(session.mapData);

        LastUpdateTimestamp = Date.now();
    });
}