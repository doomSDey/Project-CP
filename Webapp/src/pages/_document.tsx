import { Head, Html, Main, NextScript } from 'next/document';

export default function Document() {
    return (
        <Html lang="en">
            <Head>
                <link rel="icon" href="/logo.png" type="image/png" sizes="16x16" />
                <meta name="description" content="Task Management App" />
                <meta name="theme-color" content="#fff7e9" />
            </Head>
            <body>
                <Main />
                <NextScript />
            </body>
        </Html>
    );
}
