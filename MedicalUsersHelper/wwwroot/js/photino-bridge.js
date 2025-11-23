/**
 * Photino Bridge - Reusable helper for communicating with C# backend
 *
 * Usage:
 * - Send to C#: photinoBridge.send('command', payload)
 * - Listen from C#: photinoBridge.on('command', callback)
 * - Request/Response: const result = await photinoBridge.request('command', payload)
 */

class PhotinoBridge {
    constructor() {
        this.listeners = new Map();
        this.pendingRequests = new Map();
        this.requestId = 0;

        // Set up receiver
        window.external.receiveMessage((message) => {
            this.handleMessage(message);
        });

        console.log('[PhotinoBridge] Initialized');
    }

    /**
     * Send a message to C#
     * @param {string} command - The command name
     * @param {any} payload - The payload (will be JSON stringified if object)
     */
    send(command, payload = '') {
        debugger;
        let message;

        if (typeof payload === 'object') {
            message = `${command}:${JSON.stringify(payload)}`;
        } else {
            message = `${command}:${payload}`;
        }

        console.log('[PhotinoBridge] Sending:', message);
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
        console.log(`[PhotinoBridge] Registered listener for: ${command}`);
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
    request(command, payload = '', timeout = 10000) {
        return new Promise((resolve, reject) => {
            debugger;
            const requestCommand = `${command}:request:${this.requestId}`;

            // Store the pending request
            const timer = setTimeout(() => {
                this.pendingRequests.delete(this.requestId);
                reject(new Error(`Request timeout: ${command}`));
            }, timeout);

            this.pendingRequests.set(this.requestId, { resolve, reject, timer, command });

            // Send the request
            this.send(requestCommand, payload);
        });
    }

    /**
     * Handle incoming messages from C#
     * @private
     */
    handleMessage(message) {
        debugger;
        console.log('[PhotinoBridge] Received:', message);

        // Message format: "command:response:id:json" or "command:json"
        // We need to intelligently parse this

        const parts = message.split(':');
        if (parts.length < 2) {
            console.warn('[PhotinoBridge] Invalid message format:', message);
            return;
        }

        const command = parts[0];

        // Check if this is a response format: "command:response:id:json"
        if (parts.length >= 4 && parts[1] === 'response') {
            const id = parseInt(parts[2]);
            // Everything after "command:response:id:" is the JSON
            const jsonStart = parts[0].length + parts[1].length + parts[2].length + 3; // +3 for the three colons
            const payloadStr = message.substring(jsonStart);

            let payload;
            try {
                payload = JSON.parse(payloadStr);
            } catch (e) {
                console.error('[PhotinoBridge] Failed to parse response JSON:', payloadStr, e);
                payload = payloadStr;
            }

            const pending = this.pendingRequests.get(id);
            if (pending) {
                clearTimeout(pending.timer);
                this.pendingRequests.delete(id);

                if (payload.success === false || payload.error) {
                    pending.reject(new Error(payload.error || 'Request failed'));
                } else {
                    pending.resolve(payload);
                }
            }
            return;
        }

        // Regular message format: "command:payload"
        const separatorIndex = message.indexOf(':');
        const payloadStr = message.substring(separatorIndex + 1);

        // Try to parse as JSON, otherwise use as string
        let payload;
        try {
            payload = JSON.parse(payloadStr);
        } catch {
            payload = payloadStr;
        }

        // Trigger all listeners for this command
        if (this.listeners.has(command)) {
            this.listeners.get(command).forEach(callback => {
                try {
                    callback(payload);
                } catch (error) {
                    console.error(`[PhotinoBridge] Error in listener for ${command}:`, error);
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
const photinoBridge = new PhotinoBridge();

// Also expose as window.photino for convenience
window.photino = photinoBridge;