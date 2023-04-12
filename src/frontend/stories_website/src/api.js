import axios from 'axios';
import useSWR from 'swr';
import { transformLocale } from './utils';

export function useStories(pageIndex, locale, pageSize, lastKey) {
    const fetcher = async () => storiesFetcher(locale, pageSize, lastKey);

    const { data, error, isLoading } = useSWR(`/api/stories?page=${pageIndex}`, fetcher, {refreshInterval: 0, revalidateIfStale: false, revalidateOnFocus: false, revalidateOnReconnect: false})

    return {
        data: data,
        isLoading,
        isError: error
    }
}

export function storiesFetcher(locale, pageSize, lastKey) {
    const language = transformLocale(locale);
    let url = `https://lwtmylvikd.execute-api.us-east-1.amazonaws.com/Development/api/stories?language=${language}&pageSize=${pageSize}`;
    if (lastKey) {
        url += `&lastKey=${lastKey}`;
    }

    console.log(`URL: ${url}`);

    return axios.get(url).then(res => res.data);
}