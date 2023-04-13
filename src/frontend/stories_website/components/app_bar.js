import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Toolbar from '@mui/material/Toolbar';

export default function MyAppBar({children}) {
    return (
        <Box sx={{ flexGrow: 1 }}>
            <AppBar position="static" color="primary">
                <Toolbar>
                    <Box display='flex' flexGrow={1}/>
                    {children}
                </Toolbar>
            </AppBar>
        </Box>
    );
}