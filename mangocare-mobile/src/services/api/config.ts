//IMPORTANT: Change the IP address to your laptop's IP address when testing on a physical device, by checking it with the command "ipconfig" in the terminal. The IP address should be in the format "192.168.x.x" or "10.x.x.x". (IPv4 Address..........)

// Local fallback API URL.
// For physical-device development, EXPO_PUBLIC_API_URL can be set in .env
// to an HTTPS development tunnel or, later, the deployed API URL.

const LOCAL_API_URL = "http://192.168.1.100:5108/api";

export const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL || LOCAL_API_URL;


//run the app with: npx expo start --tunnel --clear
