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

function Page({ index, lastKey, setLastKey }) {
    console.log(`Page ${index} lastKey: ${lastKey}`);

    const router = useRouter()
    const { data, error, isLoading } = useStories(index, router.locale, 10, lastKey)

    console.log(`Page ${index} lastKey: ${data?.Page.LastEvaluatedKey}`)

    const stories = data?.Page.Stories ?? []

    setLastKey(data?.Page.LastEvaluatedKey ?? null);

    return (
        stories.map((story) => (
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
        ))
    )
}

// create MUI Grid with up to 2 columns from stories data 
function StoriesGrid() {
    const [cnt, setCnt] = useState(1)
    const [lastKey, setLastKey] = useState(null)
    const pages = []
    for (let i = 0; i < cnt; i++) {
        pages.push(<Page index={i} key={i} lastKey={lastKey} setLastKey={setLastKey} />)
    }

    return (
        <div className={styles.container}>
            <Grid container spacing={2}>
                {pages}
                <div className={styles.moreStories} >
                    <Button variant="contained" onClick={() => setCnt(cnt + 1)}>Load More...</Button>
                </div>                
            </Grid>
        </div>
    )
}

export default StoriesGrid;