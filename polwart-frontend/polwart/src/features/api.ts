import { useSessionStore } from '@/entities/store';
import * as signalr from '@/features/signalr';

let session: any;

//TODO: URLS in environment or config file

const backendUrl = 'https://localhost:7238';
const mediaUrl = 'http://192.168.1.162:8081/media';

let MapId: number = -1;
let LastUpdateTimestamp = 0;

export async function MediaUpload(fileName: string, file: File) {
    fetch(mediaUrl + `/${fileName}`, {
        method: 'PUT',
        body: file
    });
}

export async function Connect(mapId: number) {
    await signalr.Init();

    session = useSessionStore();
    MapId = mapId;
    let status: number;

    fetch(backendUrl + '/map/connect', {
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

        data.revisions.forEach((revision: string) => {
            session.patch(revision);
        });

        console.log(session.mapData);

        signalr.Subscribe(MapId);
    });
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
    .then(response => response.json())
    .then(data => {
        //console.log(data);
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

        console.log(data);

        data.forEach((revision: string) => {
            session.patch(revision);
        });

        console.log(session.mapData);

        LastUpdateTimestamp = Date.now();
    });
}