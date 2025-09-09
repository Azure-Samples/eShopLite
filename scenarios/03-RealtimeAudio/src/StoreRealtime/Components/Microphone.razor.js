export async function start(componentInstance) {
    try {

        // Get the list of media devices
        const devices = await navigator.mediaDevices.enumerateDevices();

        // Filter the list to find audio input devices (microphones)
        const microphones = devices.filter(device => device.kind === 'audioinput');

        // Log the names of the microphones
        microphones.forEach(mic => {
            console.log('Microphone:', mic.label);
            console.log('Microphone Device Id:', mic.deviceId);
        });

        const micStream = await navigator.mediaDevices.getUserMedia({ 
            video: false, 
            audio: { 
                deviceId: (await navigator.mediaDevices.enumerateDevices())
                    .find(device => device.kind === 'audioinput' && device.deviceId !== 'default').deviceId,
                sampleRate: 16000 
            } 
        });

        console.log('Mic stream:', micStream);
        

        processMicrophoneData(micStream, componentInstance);
        return micStream;
    } catch (ex) {
        throw new Error(`Unable to access microphone: ${ex.toString()}`);
    }
}

async function processMicrophoneData(micStream, componentInstance) {
    const audioCtx = new AudioContext({ sampleRate: 24000 });
    const micStreamSource = audioCtx.createMediaStreamSource(micStream);

    const workletBlobUrl = URL.createObjectURL(new Blob([`
        registerProcessor('sendAudioDataWorklet', class param extends AudioWorkletProcessor {
            constructor() { super(); }
            process(input, output, parameters) {
              this.port.postMessage(input[0]);
              return true;
            }
          });
        `],
        { type: 'application/javascript' }));
    await audioCtx.audioWorklet.addModule(workletBlobUrl);
    const workletNode = new AudioWorkletNode(audioCtx, 'sendAudioDataWorklet', {});
    micStreamSource.connect(workletNode);
    workletNode.port.onmessage = async (e) => {
        // We get float32, but need int16
        const float32Samples = e.data[0];
        const numSamples = float32Samples.length;
        const int16Samples = new Int16Array(numSamples);
        for (let i = 0; i < numSamples; i++) {
            int16Samples[i] = float32Samples[i] * 0x7FFF;
        }

        componentInstance.invokeMethodAsync('ReceiveAudioDataAsync', new Uint8Array(int16Samples.buffer));
    }

    await componentInstance.invokeMethodAsync('OnMicConnectedAsync');
}
