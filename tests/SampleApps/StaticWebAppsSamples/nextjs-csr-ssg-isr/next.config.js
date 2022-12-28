module.exports = {
  eslint: {
    dirs: ['src'],
  },
  output: 'standalone',
  experimental: {
    // Defaults to 50MB
    isrMemoryCacheSize: 0,
  },
};
