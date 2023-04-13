import styles from './stories_grid.module.css'
import Grid from '@mui/material/Grid';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import CardActions from '@mui/material/CardActions';
import Typography from '@mui/material/Typography';
import { useStories } from '../src/api';
import { useRouter } from 'next/router'
import { useEffect, useState } from 'react';
import Container from '@mui/material/Container';
import Button from '@mui/material/Button';

const PAGE_SIZE = 10;

// create MUI Grid with up to 2 columns from stories data 
function StoriesGrid() {
    const router = useRouter()
    const { data, isLoading, isValidating, mutate, size, setSize } = useStories(router.locale, PAGE_SIZE);

    console.log(`StoriesGrid size: ${size}`);
    console.log('StoriesGrid data: ', data);

    const stories = data ? [].concat(...data?.["Page"]["Stories"]) : [];
    const isLoadingMore = isLoading || (size > 0 && data && typeof data[size - 1] === "undefined");
    const isEmpty = data?.[0]?.length === 0;
    const isReachingEnd = isEmpty || (data && !data.Page.HasMore);
    const isRefreshing = isValidating && data && data.length === size;

    return (
        <div className={styles.container}>
            <Grid container spacing={2}>
                {stories.map((story) => (
                    <Grid item xs={12} sm={6} key={story.StoryId}>
                        <Card variant="outlined">
                            <CardActions disableSpacing>

                            </CardActions>
                            <CardContent>
                                {/* <Typography sx={{ fontSize: 14 }} color="text.secondary" gutterBottom>
                                    {story["StoryId"]}
                                </Typography> */}
                                <Typography variant="body1" className={styles.storyText} display="block">
                                    {story["Story"]}
                                </Typography>
                            </CardContent>
                        </Card>
                    </Grid>
                ))}
                <div className={styles.moreStories} >
                    <Button variant="contained" onClick={() => setSize(size + 1)} disabled={isLoadingMore || isReachingEnd}>
                        {isLoadingMore
                            ? "Loading..."
                            : isReachingEnd
                                ? "No more stories"
                                : "Load more..."}
                    </Button>
                </div>
            </Grid>
        </div>
    )
}

export default StoriesGrid;