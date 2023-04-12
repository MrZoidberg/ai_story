import axios from 'axios';
import useSWR from 'swr';
import { transformLocale } from './utils';

export function useStories(locale, pageSize, lastKey) {
    const language = transformLocale(locale);

    var url = `https://lwtmylvikd.execute-api.us-east-1.amazonaws.com/Development/api/stories?language=${language}&pageSize=${pageSize}`;
    if (lastKey) {
        url += `&lastKey=${lastKey}`;
    }

    const fetcher = async () => axios(url).then(response => response.data);

    console.log(`URL: ${url}`);

    const { data, error, isLoading } = useSWR('/api/stories', fetcher, {refreshInterval: 0, revalidateIfStale: false, revalidateOnFocus: false, revalidateOnReconnect: false})

    return {
        data: data,
        isLoading,
        isError: error
    }
}

export function storiesFetcher(locale, pageSize, lastKey) {
    const language = transformLocale(locale);
    var url = `https://lwtmylvikd.execute-api.us-east-1.amazonaws.com/Development/api/stories?language=${language}&pageSize=${pageSize}`;
    if (lastKey) {
        url += `&lastKey=${lastKey}`;
    }

    console.log(`Fallback URL: ${url}`);

    return axios.get(url).then(res => res.data);
}

// export default async function fetcher(...args) {
//     const res = await fetch(...args)
//     return res.json()
// }