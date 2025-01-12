import withBundleAnalyzer from '@next/bundle-analyzer';
import i18n from './next-i18next.config.mjs';

const bundleAnalyzer = withBundleAnalyzer({
  enabled: process.env.ANALYZE === 'true',
});

export default bundleAnalyzer({
  poweredByHeader: false,
  trailingSlash: true,
  basePath: '',
  reactStrictMode: true,
  images: {
    domains: ['storage.googleapis.com'],
  },
  i18n,
});
