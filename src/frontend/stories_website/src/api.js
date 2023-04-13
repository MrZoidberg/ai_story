import axios from 'axios';
import useSWRInfinite from 'swr/infinite';
import { transformLocale } from './utils';
import { unstable_serialize } from "swr/infinite";


const getKey = (pageIndex, locale, pageSize, previousPageData) => {
    console.log(`getKey: ${pageIndex}, ${locale}, ${pageSize}, ${previousPageData}`);

    const language = transformLocale(locale);
    if (previousPageData && !previousPageData.Page.HasMore) return null; // reached the end
    // first page, we don't have `previousPageData`
    if (pageIndex === 0) return `https://lwtmylvikd.execute-api.us-east-1.amazonaws.com/Development/api/stories?language=${language}&pageSize=${pageSize}`

    // add the cursor to the API endpoint
    return `https://lwtmylvikd.execute-api.us-east-1.amazonaws.com/Development/api/stories?language=${language}&pageSize=${pageSize}&lastKey=${previousPageData.Page.LastEvaluatedKey}`
}

export function useStories(locale, pageSize) {
    const fetcher = (url) => axios.get(url).then(res => res.data);

    const { data, mutate, size, setSize, isValidating, isLoading} = useSWRInfinite((pageIndex, previousPageData) => {return getKey(pageIndex, locale, pageSize, previousPageData)}, fetcher, { initialSize: 1, revalidateFirstPage: false })

    return {
        data,        
        isLoading,
        isValidating,
        mutate: mutate,
        size,
        setSize,
    }
}

export async function storiesFetcher(locale, pageSize) {
    const url = getKey(0, locale, pageSize, null)
    return {url: (page) => getKey(page, locale, pageSize, null) , data: await axios.get(url).then(res => res.data)}
}