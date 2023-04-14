/**
 * @type {import('next').NextConfig}
 */
const nextConfig = {
    /* config options here */
    swcMinify: true,
    images: {
        // Nowhere to cache the images in Lambda (read only)
        unoptimized: true, // Next 12.3+, other "experimental -> images -> unoptimized"
    },
    output: "export", // THIS IS IMPORTANT
    compress: false,
    distDir: 'dist'
}

module.exports = nextConfig