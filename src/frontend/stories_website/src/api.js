import axios from 'axios';
import useSWRInfinite from 'swr/infinite';
import { transformLocale } from './utils';
import { unstable_serialize } from "swr/infinite";

// axios.interceptors.request.use(request => {
//     console.log('Starting Request', JSON.stringify(request, null, 2))
//     return request
//   })
  
//   axios.interceptors.response.use(response => {
//     console.log('Response:', JSON.stringify(response, null, 2))
//     return response
//   })


const getKey = (pageIndex, locale, pageSize, previousPageData) => {
    //console.log(`getKey: ${pageIndex}, ${locale}, ${pageSize} `, previousPageData);

    const language = transformLocale(locale);
    if (previousPageData && !previousPageData.Page.HasMore) return null; // reached the end
    // first page, we don't have `previousPageData`
    if (pageIndex === 0) return `https://lwtmylvikd.execute-api.us-east-1.amazonaws.com/Development/api/stories?language=${language}&pageSize=${pageSize}`

    // add the cursor to the API endpoint
    return `https://lwtmylvikd.execute-api.us-east-1.amazonaws.com/Development/api/stories?language=${language}&pageSize=${pageSize}&lastKey=${previousPageData.Page.LastEvaluatedKey}`
}

export function useStories(locale, pageSize) {
    const fetcher = (url) => axios.get(url).then(res => res.data);
    return useSWRInfinite((pageIndex, previousPageData) => {return getKey(pageIndex, locale, pageSize, previousPageData)}, fetcher, { revalidateFirstPage: false, revalidateAll: false, persistSize: true })
}

export async function storiesFetcher(locale, pageSize) {
    const url = getKey(0, locale, pageSize, null)
    return {url: (page) => getKey(page, locale, pageSize, null) , data: await axios.get(url).then(res => {
        return res.data
    })}
}