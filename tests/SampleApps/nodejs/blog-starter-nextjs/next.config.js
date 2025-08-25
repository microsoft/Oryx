const withMDX = require('@next/mdx')({
  extension: /\.mdx?$/,
  options: {
    // Configure MDX options here if needed
  }
})

/** @type {import('next').NextConfig} */
const nextConfig = {
  pageExtensions: ['js', 'jsx', 'mdx', 'md'],
  output: 'standalone', // Modern replacement for 'serverless'
}

module.exports = withMDX(nextConfig)