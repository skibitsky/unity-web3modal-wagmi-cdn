mergeInto(LibraryManager.library, {
    // Global variable to store the loaded modules and configuration
    _web3ModalConfig: null,

    // Method to preload the scripts from CDN
    PreloadWeb3Modal: function (projectIdPtr, appNameStrPtr, appLogoUrlStrPtr) {
        var projectId = UTF8ToString(projectIdPtr);
        var appName = UTF8ToString(appNameStrPtr);
        var appLogoUrl = UTF8ToString(appLogoUrlStrPtr);
        
        console.log("Preloading Web3Modal with Project ID:", projectId);

        // Load the scripts and initialize the configuration
        import("https://cdn.jsdelivr.net/npm/cdn-wagmi@3.0.0/dist/cdn-wagmi.js").then(CDNW3M => {
            const { WagmiCore, Chains, Web3modal, Connectors } = CDNW3M;
            const { createWeb3Modal, defaultWagmiConfig } = Web3modal;
            const { mainnet, sepolia } = Chains;
            const { coinbaseWallet, walletConnect, injected } = Connectors;
            const { createConfig, http, reconnect } = WagmiCore;
            
            console.log("Web3Modal loaded successfully");

            const metadata = {
                name: appName,
                description: 'Web3Modal Example',
                url: 'https://web3modal.com', // url must match your domain & subdomain
                icons: [appLogoUrl]
            };

            const config = createConfig({
                chains: [mainnet, sepolia],
                transports: {
                    [mainnet.id]: http(),
                    [sepolia.id]: http()
                },
                connectors: [
                    walletConnect({ projectId, metadata, showQrModal: false }),
                    injected({ shimDisconnect: true }),
                    coinbaseWallet({
                        appName: metadata.name,
                        appLogoUrl: metadata.icons[0]
                    })
                ]
            });
            
            console.log("Web3Modal configuration loaded successfully");

            reconnect(config);

            const modal = createWeb3Modal({
                wagmiConfig: config,
                projectId,
                enableAnalytics: true, // Optional - defaults to your Cloud configuration
                enableOnramp: true // Optional - false as default
            });

            console.log("Web3Modal modal created successfully", modal);
            
            // Store the configuration and modal globally
            _web3ModalConfig = {
                config: config,
                modal: modal,
                wagmiCore: WagmiCore
            };
        });
    },
    OpenWeb3Modal: function () {
        console.log("Opening Web3Modal", _web3ModalConfig);
        if (_web3ModalConfig) {
            _web3ModalConfig.modal.open();
        } else {
            console.error("Web3Modal is not initialized. Call PreloadWeb3Modal first.");
        }
    },
    WagmiCall: async function(id, methodNameStrPtr, parameterStrPtr, callbackPtr) {
        if (_web3ModalConfig) {
            var methodName = UTF8ToString(methodNameStrPtr);
            var parameterStr = UTF8ToString(parameterStrPtr);
            var parameter = JSON.parse(parameterStr);
            
            try {
                var result = await _web3ModalConfig.wagmiCore[methodName](_web3ModalConfig.config, parameter);
                
                var resultJson = JSON.stringify(result);
                var resultStrBuffer = stringToNewUTF8(resultJson);
                {{{ makeDynCall('viii', 'callbackPtr') }}} (id, resultStrBuffer, undefined);
                _free(resultStrBuffer);
            } catch (error) {
                var errorJson = JSON.stringify(error, ['name', 'message']);
                var errorStrBuffer = stringToNewUTF8(errorJson);
                {{{ makeDynCall('viii', 'callbackPtr') }}} (id, undefined, errorStrBuffer);
                _free(errorStrBuffer);
            }
        } else {
            console.error("Web3Modal is not initialized. Call PreloadWeb3Modal first.");
        }
    }
});
