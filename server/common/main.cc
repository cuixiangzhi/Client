
#include <gl/glut.h>
#include <windows.h>

void render_loop()
{

}

void render_init(int argc, char** argv)
{
	glutInit(&argc, argv);

	int widthSys = GetSystemMetrics(SM_CXSCREEN);
	int heightSys = GetSystemMetrics(SM_CYSCREEN);

	int widthApp = 960;
	int heightApp = 540;

	int posx = widthSys * 0.5 - widthApp * 0.5;
	int posy = heightSys * 0.5 - heightApp * 0.5;

	glutInitWindowSize(widthApp, heightApp);
	glutInitWindowPosition(posx, posy);
	glutCreateWindow("Unity");

	glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGBA | GLUT_DEPTH | GLUT_STENCIL | GLUT_ALPHA);
	glutDisplayFunc(render_loop);

	const GLubyte* version = glGetString(GL_VERSION);

	glutMainLoop();
}

void server_loop()
{

}

void server_init(int argc, char** argv)
{
	//WORD wd = MAKEWORD(2, 2);
	//LPWSADATA lpwsaData = NULL;
	//if (WSAStartup(wd, lpwsaData) != 0)
	//{
	//	return;
	//}
	//hostent* host = gethostbyname("www.baidu.com");
	//int x = 0;
}

int main(int argc, char** argv)
{
	render_init(argc,argv);
	server_init(argc, argv);
}