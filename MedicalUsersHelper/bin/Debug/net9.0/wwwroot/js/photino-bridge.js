/**
 * Photino Bridge - Reusable helper for communicating with C# backend
 *
 * Usage:
 * - Send to C#: photinoBridge.send('command', payload)
 * - Listen from C#: photinoBridge.on('command', callback)
 * - Request/Response: const result = await photinoBridge.request('command', payload)
 */

class PhotoinoBridge {
    constructor() {
        this.listeners = new Map();
        this.pendingRequests = new Map();
        this.requestId = 0;

        // Set up receiver
        window.external.receiveMessage((message) => {
            this.handleMessage(message);
        });

        console.log('[PhotoinoBridge] Initialized');
    }

    /**
     * Send a message to C#
     * @param {string} command - The command name
     * @param {any} payload - The payload (will be JSON stringified if object)
     */
    send(command, payload = '') {
        let message;

        if (typeof payload === 'object') {
            message = `${command}:${JSON.stringify(payload)}`;
        } else {
            message = `${command}:${payload}`;
        }

        console.log('[PhotoinoBridge] Sending:', message);
        window.external.sendMessage(message);
    }

    /**
     * Listen for messages from C# with a specific command
     * @param {string} command - The command to listen for
     * @param {Function} callback - Callback function (data) => {}
     */
    on(command, callback) {
        if (!this.listeners.has(command)) {
            this.listeners.set(command, []);
        }
        this.listeners.get(command).push(callback);
        console.log(`[PhotoinoBridge] Registered listener for: ${command}`);
    }

    /**
     * Remove a listener
     * @param {string} command - The command
     * @param {Function} callback - The callback to remove
     */
    off(command, callback) {
        if (!this.listeners.has(command)) return;

        const callbacks = this.listeners.get(command);
        const index = callbacks.indexOf(callback);
        if (index > -1) {
            callbacks.splice(index, 1);
        }
    }

    /**
     * Send a request and wait for a response (Promise-based)
     * @param {string} command - The command
     * @param {any} payload - The payload
     * @param {number} timeout - Timeout in ms (default 5000)
     * @returns {Promise<any>} The response data
     */
    request(command, payload = '', timeout = 5000) {
        return new Promise((resolve, reject) => {
            const id = ++this.requestId;
            const requestCommand = `${command}:request:${id}`;

            // Store the pending request
            const timer = setTimeout(() => {
                this.pendingRequests.delete(id);
                reject(new Error(`Request timeout: ${command}`));
            }, timeout);

            this.pendingRequests.set(id, { resolve, reject, timer, command });

            // Send the request
            this.send(requestCommand, payload);
        });
    }

    /**
     * Handle incoming messages from C#
     * @private
     */
    handleMessage(message) {
        console.log('[PhotoinoBridge] Received:', message);

        const separatorIndex = message.indexOf(':');
        if (separatorIndex === -1) {
            console.warn('[PhotoinoBridge] Invalid message format:', message);
            return;
        }

        const command = message.substring(0, separatorIndex);
        const payloadStr = message.substring(separatorIndex + 1);

        // Try to parse as JSON, otherwise use as string
        let payload;
        try {
            payload = JSON.parse(payloadStr);
        } catch {
            payload = payloadStr;
        }

        // Check if this is a response to a pending request
        const requestMatch = command.match(/^(.+):response:(\d+)$/);
        if (requestMatch) {
            const [, originalCommand, id] = requestMatch;
            const pending = this.pendingRequests.get(parseInt(id));

            if (pending) {
                clearTimeout(pending.timer);
                this.pendingRequests.delete(parseInt(id));

                if (payload.success === false || payload.error) {
                    pending.reject(new Error(payload.error || 'Request failed'));
                } else {
                    pending.resolve(payload);
                }
            }
            return;
        }

        // Trigger all listeners for this command
        if (this.listeners.has(command)) {
            this.listeners.get(command).forEach(callback => {
                try {
                    callback(payload);
                } catch (error) {
                    console.error(`[PhotoinoBridge] Error in listener for ${command}:`, error);
                }
            });
        }
    }

    /**
     * Helper: Parse data from various formats
     * @private
     */
    parseData(data) {
        if (typeof data === 'string') {
            try {
                return JSON.parse(data);
            } catch {
                return data;
            }
        }
        return data;
    }
}

// Create global instance
const photinoBridge = new PhotoinoBridge();

// Also expose as window.photino for convenience
window.photino = photinoBridge;