import styles from './stories_grid.module.css'
import Grid from '@mui/material/Grid';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import CardActions from '@mui/material/CardActions';
import Typography from '@mui/material/Typography';
import { useStories } from '../src/api';
import { useRouter } from 'next/router'


// create MUI Grid with up to 2 columns from stories data 
function StoriesGrid() {
    const router = useRouter()
    const { data, error, isLoading } = useStories(router.locale, 10, null)

    const stories = data.Page.Stories

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
            </Grid>
        </div>
    )
}

export default StoriesGrid;