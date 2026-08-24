import { Platform } from 'react-native';

function configuredUrl(value: unknown, fallback: string) {
  return (typeof value === 'string' && value ? value : fallback).replace(/\/+$/, '');
}

export const apiBaseUrl = __DEV__
  ? Platform.OS === 'android'
    ? 'http://10.0.2.2:5264'
    : 'http://127.0.0.1:5264'
  : configuredUrl(process.env.EXPO_PUBLIC_LIFTLOG_API_URL, 'https://divefrom.space/liftlog-api');

export const shareAppBaseUrl = configuredUrl(
  process.env.EXPO_PUBLIC_LIFTLOG_SHARE_APP_URL,
  'https://divefrom.space/liftlog',
);
