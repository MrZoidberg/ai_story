/**
 * @type {import('next').NextConfig}
 */
const nextConfig = {
    /* config options here */
    i18n: {
        locales: ['en', 'ru', 'ua'],
        defaultLocale: 'en',
    },
    swcMinify: true,
    images: {
        // Nowhere to cache the images in Lambda (read only)
        unoptimized: true, // Next 12.3+, other "experimental -> images -> unoptimized"
    },
    output: "standalone", // THIS IS IMPORTANT
    compress: false,
}

module.exports = nextConfig